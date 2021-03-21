// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#nullable enable

using System.Management.Automation.Internal;
using System.Management.Automation.Language;

namespace System.Management.Automation.Subsystem
{
    /// <summary>
    /// The class represents the prediction result from a predictor.
    /// </summary>
    public sealed class PredictionResult
    {
        /// <summary>
        /// Gets the Id of the predictor.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Gets the name of the predictor.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the mini-session id that represents a specific invocation to the <see cref="ICommandPredictor.GetSuggestion"/> API of the predictor.
        /// When it's not specified, it's considered by a client that the predictor doesn't expect feedback.
        /// </summary>
        public uint? Session { get; }

        /// <summary>
        /// Gets the suggestions.
        /// </summary>
        public IReadOnlyList<PredictiveSuggestion> Suggestions { get; }

        internal PredictionResult(Guid id, string name, uint? session, List<PredictiveSuggestion> suggestions)
        {
            Id = id;
            Name = name;
            Session = session;
            Suggestions = suggestions;
        }
    }

    /// <summary>
    /// Provides a set of possible predictions for given input.
    /// </summary>
    public static class CommandPrediction
    {
        /// <summary>
        /// Collect the predictive suggestions from registered predictors using the default timeout.
        /// </summary>
        /// <param name="client">Represents the client that initiates the call.</param>
        /// <param name="ast">The <see cref="Ast"/> object from parsing the current command line input.</param>
        /// <param name="astTokens">The <see cref="Token"/> objects from parsing the current command line input.</param>
        /// <returns>A list of <see cref="PredictionResult"/> objects.</returns>
        public static Task<List<PredictionResult>?> PredictInput(string client, Ast ast, Token[] astTokens)
        {
            return PredictInput(client, ast, astTokens, millisecondsTimeout: 20);
        }

        /// <summary>
        /// Collect the predictive suggestions from registered predictors using the specified timeout.
        /// </summary>
        /// <param name="client">Represents the client that initiates the call.</param>
        /// <param name="ast">The <see cref="Ast"/> object from parsing the current command line input.</param>
        /// <param name="astTokens">The <see cref="Token"/> objects from parsing the current command line input.</param>
        /// <param name="millisecondsTimeout">The milliseconds to timeout.</param>
        /// <returns>A list of <see cref="PredictionResult"/> objects.</returns>
        public static async Task<List<PredictionResult>?> PredictInput(string client, Ast ast, Token[] astTokens, int millisecondsTimeout)
        {
            Requires.Condition(millisecondsTimeout > 0, nameof(millisecondsTimeout));

            var predictors = SubsystemManager.GetSubsystems<ICommandPredictor>();
            if (predictors.Count == 0)
            {
                return null;
            }

            var context = new PredictionContext(ast, astTokens);
            var tasks = new Task<PredictionResult?>[predictors.Count];
            using var cancellationSource = new CancellationTokenSource();

            for (int i = 0; i < predictors.Count; i++)
            {
                ICommandPredictor predictor = predictors[i];

                tasks[i] = Task.Factory.StartNew(
                    state =>
                    {
                        var predictor = (ICommandPredictor)state!;
                        SuggestionPackage pkg = predictor.GetSuggestion(client, context, cancellationSource.Token);
                        return pkg.SuggestionEntries?.Count > 0 ? new PredictionResult(predictor.Id, predictor.Name, pkg.Session, pkg.SuggestionEntries) : null;
                    },
                    predictor,
                    cancellationSource.Token,
                    TaskCreationOptions.DenyChildAttach,
                    TaskScheduler.Default);
            }

            await Task.WhenAny(
                Task.WhenAll(tasks),
                Task.Delay(millisecondsTimeout, cancellationSource.Token)).ConfigureAwait(false);
            cancellationSource.Cancel();

            var resultList = new List<PredictionResult>(predictors.Count);
            foreach (Task<PredictionResult?> task in tasks)
            {
                if (task.IsCompletedSuccessfully)
                {
                    PredictionResult? result = task.Result;
                    if (result != null)
                    {
                        resultList.Add(result);
                    }
                }
            }

            return resultList;
        }

        /// <summary>
        /// Allow registered predictors to do early processing when a command line is accepted.
        /// </summary>
        /// <param name="client">Represents the client that initiates the call.</param>
        /// <param name="history">History command lines provided as references for prediction.</param>
        public static void OnCommandLineAccepted(string client, IReadOnlyList<string> history)
        {
            Requires.NotNull(history, nameof(history));

            var predictors = SubsystemManager.GetSubsystems<ICommandPredictor>();
            if (predictors.Count == 0)
            {
                return;
            }

            foreach (ICommandPredictor predictor in predictors)
            {
                if (predictor.SupportEarlyProcessing)
                {
                    ThreadPool.QueueUserWorkItem<ICommandPredictor>(
                        state => state.StartEarlyProcessing(client, history),
                        predictor,
                        preferLocal: false);
                }
            }
        }

        /// <summary>
        /// Send feedback to a predictor when one or more suggestions from it were displayed to the user.
        /// </summary>
        /// <param name="client">Represents the client that initiates the call.</param>
        /// <param name="predictorId">The identifier of the predictor whose prediction result was accepted.</param>
        /// <param name="session">The mini-session where the displayed suggestions came from.</param>
        /// <param name="countOrIndex">
        /// When the value is greater than 0, it's the number of displayed suggestions from the list returned in <paramref name="session"/>, starting from the index 0.
        /// When the value is less than or equal to 0, it means a single suggestion from the list got displayed, and the index is the absolute value.
        /// </param>
        public static void OnSuggestionDisplayed(string client, Guid predictorId, uint session, int countOrIndex)
        {
            var predictors = SubsystemManager.GetSubsystems<ICommandPredictor>();
            if (predictors.Count == 0)
            {
                return;
            }

            foreach (ICommandPredictor predictor in predictors)
            {
                if (predictor.AcceptFeedback && predictor.Id == predictorId)
                {
                    ThreadPool.QueueUserWorkItem<ICommandPredictor>(
                        state => state.OnSuggestionDisplayed(client, session, countOrIndex),
                        predictor,
                        preferLocal: false);
                }
            }
        }

        /// <summary>
        /// Send feedback to a predictor when a suggestion from it was accepted.
        /// </summary>
        /// <param name="client">Represents the client that initiates the call.</param>
        /// <param name="predictorId">The identifier of the predictor whose prediction result was accepted.</param>
        /// <param name="session">The mini-session where the accepted suggestion came from.</param>
        /// <param name="suggestionText">The accepted suggestion text.</param>
        public static void OnSuggestionAccepted(string client, Guid predictorId, uint session, string suggestionText)
        {
            Requires.NotNullOrEmpty(suggestionText, nameof(suggestionText));

            var predictors = SubsystemManager.GetSubsystems<ICommandPredictor>();
            if (predictors.Count == 0)
            {
                return;
            }

            foreach (ICommandPredictor predictor in predictors)
            {
                if (predictor.AcceptFeedback && predictor.Id == predictorId)
                {
                    ThreadPool.QueueUserWorkItem<ICommandPredictor>(
                        state => state.OnSuggestionAccepted(client, session, suggestionText),
                        predictor,
                        preferLocal: false);
                }
            }
        }
    }
}
