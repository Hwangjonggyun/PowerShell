// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

/********************************************************************++

    Project:     PowerShell

    Contents:    PowerShell token interface for syntax editors

    Classes:     System.Management.Automation.PSToken

--********************************************************************/

using System.Management.Automation.Language;

namespace System.Management.Automation
{
    /// <summary>
    /// This is public class for representing a powershell token.
    /// </summary>
    /// <remarks>
    /// There is already an internal class Token for representing the token.
    ///
    /// This class wraps the internal Token class for providing limited information
    /// to syntax editor.
    /// </remarks>
    public sealed class PSToken
    {
        internal PSToken(Token token)
        {
            Type = GetPSTokenType(token);
            _extent = token.Extent;
            if (token is StringToken)
            {
                _content = ((StringToken)token).Value;
            }
            else if (token is VariableToken)
            {
                _content = ((VariableToken)token).VariablePath.ToString();
            }
        }

        internal PSToken(IScriptExtent extent)
        {
            Type = PSTokenType.Position;
            _extent = extent;
        }

        /// <summary>
        /// Resulting text for the token.
        /// </summary>
        /// <remarks>
        /// The text here represents the content of token. It can be the same as
        /// the text chunk within script resulting into this token, but usually is not
        /// the case.
        ///
        /// For example, -name in following command result into a parameter token.
        ///
        ///     get-process -name foo
        ///
        /// Text property in this case is 'name' instead of '-name'.
        /// </remarks>
        public string Content
        {
            get
            {
                return _content ?? _extent.Text;
            }
        }

        private readonly string _content;

        #region Token Type

        /// <summary>
        /// Map a V3 token to a V2 PSTokenType.
        /// </summary>
        /// <param name="token">The V3 token.</param>
        /// <returns>The V2 PSTokenType.</returns>
        public static PSTokenType GetPSTokenType(Token token)
        {
            if ((token.TokenFlags & TokenFlags.CommandName) != 0)
            {
                return PSTokenType.Command;
            }

            if ((token.TokenFlags & TokenFlags.MemberName) != 0)
            {
                return PSTokenType.Member;
            }

            if ((token.TokenFlags & TokenFlags.AttributeName) != 0)
            {
                return PSTokenType.Attribute;
            }

            if ((token.TokenFlags & TokenFlags.TypeName) != 0)
            {
                return PSTokenType.Type;
            }

            return s_tokenKindMapping[(int)token.Kind];
        }

        /// <summary>
        /// Token type.
        /// </summary>
        public PSTokenType Type { get; }

        private static readonly PSTokenType[] s_tokenKindMapping = new PSTokenType[]
        {
            #region Flags for unclassified tokens

            /*              Unknown */ PSTokenType.Unknown,
            /*             Variable */ PSTokenType.Variable,
            /*     SplattedVariable */ PSTokenType.Variable,
            /*            Parameter */ PSTokenType.CommandParameter,
            /*               Number */ PSTokenType.Number,
            /*                Label */ PSTokenType.LoopLabel,
            /*           Identifier */ PSTokenType.CommandArgument,

            /*              Generic */ PSTokenType.CommandArgument,
            /*              Newline */ PSTokenType.NewLine,
            /*     LineContinuation */ PSTokenType.LineContinuation,
            /*              Comment */ PSTokenType.Comment,
            /*           EndOfInput */ PSTokenType.Unknown,

            #endregion Flags for unclassified tokens

            #region Flags for strings

            /*        StringLiteral */ PSTokenType.String,
            /*     StringExpandable */ PSTokenType.String,
            /*    HereStringLiteral */ PSTokenType.String,
            /* HereStringExpandable */ PSTokenType.String,

            #endregion Flags for strings

            #region Flags for punctuators

            /*               LParen */ PSTokenType.GroupStart,
            /*               RParen */ PSTokenType.GroupEnd,
            /*               LCurly */ PSTokenType.GroupStart,
            /*               RCurly */ PSTokenType.GroupEnd,
            /*             LBracket */ PSTokenType.Operator,
            /*             RBracket */ PSTokenType.Operator,
            /*              AtParen */ PSTokenType.GroupStart,
            /*              AtCurly */ PSTokenType.GroupStart,
            /*          DollarParen */ PSTokenType.GroupStart,
            /*                 Semi */ PSTokenType.StatementSeparator,

            #endregion Flags for punctuators

            #region Flags for operators

            /*               AndAnd */ PSTokenType.Operator,
            /*                 OrOr */ PSTokenType.Operator,
            /*            Ampersand */ PSTokenType.Operator,
            /*                 Pipe */ PSTokenType.Operator,
            /*                Comma */ PSTokenType.Operator,
            /*           MinusMinus */ PSTokenType.Operator,
            /*             PlusPlus */ PSTokenType.Operator,
            /*               DotDot */ PSTokenType.Operator,
            /*           ColonColon */ PSTokenType.Operator,
            /*                  Dot */ PSTokenType.Operator,
            /*              Exclaim */ PSTokenType.Operator,
            /*             Multiply */ PSTokenType.Operator,
            /*               Divide */ PSTokenType.Operator,
            /*                  Rem */ PSTokenType.Operator,
            /*                 Plus */ PSTokenType.Operator,
            /*                Minus */ PSTokenType.Operator,
            /*               Equals */ PSTokenType.Operator,
            /*           PlusEquals */ PSTokenType.Operator,
            /*          MinusEquals */ PSTokenType.Operator,
            /*       MultiplyEquals */ PSTokenType.Operator,
            /*         DivideEquals */ PSTokenType.Operator,
            /*      RemainderEquals */ PSTokenType.Operator,
            /*          Redirection */ PSTokenType.Operator,
            /*        RedirectInStd */ PSTokenType.Operator,
            /*               Format */ PSTokenType.Operator,
            /*                  Not */ PSTokenType.Operator,
            /*                 Bnot */ PSTokenType.Operator,
            /*                  And */ PSTokenType.Operator,
            /*                   Or */ PSTokenType.Operator,
            /*                  Xor */ PSTokenType.Operator,
            /*                 Band */ PSTokenType.Operator,
            /*                  Bor */ PSTokenType.Operator,
            /*                 Bxor */ PSTokenType.Operator,
            /*                 Join */ PSTokenType.Operator,
            /*                  Ieq */ PSTokenType.Operator,
            /*                  Ine */ PSTokenType.Operator,
            /*                  Ige */ PSTokenType.Operator,
            /*                  Igt */ PSTokenType.Operator,
            /*                  Ilt */ PSTokenType.Operator,
            /*                  Ile */ PSTokenType.Operator,
            /*                Ilike */ PSTokenType.Operator,
            /*             Inotlike */ PSTokenType.Operator,
            /*               Imatch */ PSTokenType.Operator,
            /*            Inotmatch */ PSTokenType.Operator,
            /*             Ireplace */ PSTokenType.Operator,
            /*            Icontains */ PSTokenType.Operator,
            /*         Inotcontains */ PSTokenType.Operator,
            /*                  Iin */ PSTokenType.Operator,
            /*               Inotin */ PSTokenType.Operator,
            /*               Isplit */ PSTokenType.Operator,
            /*                  Ceq */ PSTokenType.Operator,
            /*                  Cne */ PSTokenType.Operator,
            /*                  Cge */ PSTokenType.Operator,
            /*                  Cgt */ PSTokenType.Operator,
            /*                  Clt */ PSTokenType.Operator,
            /*                  Cle */ PSTokenType.Operator,
            /*                Clike */ PSTokenType.Operator,
            /*             Cnotlike */ PSTokenType.Operator,
            /*               Cmatch */ PSTokenType.Operator,
            /*            Cnotmatch */ PSTokenType.Operator,
            /*             Creplace */ PSTokenType.Operator,
            /*            Ccontains */ PSTokenType.Operator,
            /*         Cnotcontains */ PSTokenType.Operator,
            /*                  Cin */ PSTokenType.Operator,
            /*               Cnotin */ PSTokenType.Operator,
            /*               Csplit */ PSTokenType.Operator,
            /*                   Is */ PSTokenType.Operator,
            /*                IsNot */ PSTokenType.Operator,
            /*                   As */ PSTokenType.Operator,
            /*      PostFixPlusPlus */ PSTokenType.Operator,
            /*    PostFixMinusMinus */ PSTokenType.Operator,
            /*                  Shl */ PSTokenType.Operator,
            /*                  Shr */ PSTokenType.Operator,
            /*    Reserved slot 1   */ PSTokenType.Unknown,
            /*    Reserved slot 2   */ PSTokenType.Unknown,
            /*    Reserved slot 3   */ PSTokenType.Unknown,
            /*    Reserved slot 4   */ PSTokenType.Unknown,
            /*    Reserved slot 5   */ PSTokenType.Unknown,
            /*    Reserved slot 6   */ PSTokenType.Unknown,
            /*    Reserved slot 7   */ PSTokenType.Unknown,
            /*    Reserved slot 8   */ PSTokenType.Unknown,
            /*    Reserved slot 9   */ PSTokenType.Unknown,
            /*    Reserved slot 10  */ PSTokenType.Unknown,
            /*    Reserved slot 11  */ PSTokenType.Unknown,
            /*    Reserved slot 12  */ PSTokenType.Unknown,
            /*    Reserved slot 13  */ PSTokenType.Unknown,
            /*    Reserved slot 14  */ PSTokenType.Unknown,
            /*    Reserved slot 15  */ PSTokenType.Unknown,
            /*    Reserved slot 16  */ PSTokenType.Unknown,
            /*    Reserved slot 17  */ PSTokenType.Unknown,
            /*    Reserved slot 18  */ PSTokenType.Unknown,
            /*    Reserved slot 19  */ PSTokenType.Unknown,
            /*    Reserved slot 20  */ PSTokenType.Unknown,

            #endregion Flags for operators

            #region Flags for keywords

            /*                Begin */ PSTokenType.Keyword,
            /*                Break */ PSTokenType.Keyword,
            /*                Catch */ PSTokenType.Keyword,
            /*                Class */ PSTokenType.Keyword,
            /*             Continue */ PSTokenType.Keyword,
            /*                 Data */ PSTokenType.Keyword,
            /*               Define */ PSTokenType.Keyword,
            /*                   Do */ PSTokenType.Keyword,
            /*         Dynamicparam */ PSTokenType.Keyword,
            /*                 Else */ PSTokenType.Keyword,
            /*               ElseIf */ PSTokenType.Keyword,
            /*                  End */ PSTokenType.Keyword,
            /*                 Exit */ PSTokenType.Keyword,
            /*               Filter */ PSTokenType.Keyword,
            /*              Finally */ PSTokenType.Keyword,
            /*                  For */ PSTokenType.Keyword,
            /*              Foreach */ PSTokenType.Keyword,
            /*                 From */ PSTokenType.Keyword,
            /*             Function */ PSTokenType.Keyword,
            /*                   If */ PSTokenType.Keyword,
            /*                   In */ PSTokenType.Keyword,
            /*                Param */ PSTokenType.Keyword,
            /*              Process */ PSTokenType.Keyword,
            /*               Return */ PSTokenType.Keyword,
            /*               Switch */ PSTokenType.Keyword,
            /*                Throw */ PSTokenType.Keyword,
            /*                 Trap */ PSTokenType.Keyword,
            /*                  Try */ PSTokenType.Keyword,
            /*                Until */ PSTokenType.Keyword,
            /*                Using */ PSTokenType.Keyword,
            /*                  Var */ PSTokenType.Keyword,
            /*                While */ PSTokenType.Keyword,
            /*             Workflow */ PSTokenType.Keyword,
            /*             Parallel */ PSTokenType.Keyword,
            /*             Sequence */ PSTokenType.Keyword,
            /*         InlineScript */ PSTokenType.Keyword,
            /*        Configuration */ PSTokenType.Keyword,
            /*       DynamicKeyword */ PSTokenType.Keyword,
            /*               Public */ PSTokenType.Keyword,
            /*              Private */ PSTokenType.Keyword,
            /*               Static */ PSTokenType.Keyword,
            /*            Interface */ PSTokenType.Keyword,
            /*                 Enum */ PSTokenType.Keyword,
            /*            Namespace */ PSTokenType.Keyword,
            /*               Module */ PSTokenType.Keyword,
            /*                 Type */ PSTokenType.Keyword,
            /*             Assembly */ PSTokenType.Keyword,
            /*              Command */ PSTokenType.Keyword,
            /*               Hidden */ PSTokenType.Keyword,
            /*                 Base */ PSTokenType.Keyword,
            /*              Default */ PSTokenType.Keyword,

            #endregion Flags for keywords

            /*            LastToken */ PSTokenType.Unknown,
        };

        #endregion

        #region Position Information

        private readonly IScriptExtent _extent;

        /// <summary>
        /// Offset of token start in script buffer.
        /// </summary>
        public int Start
        {
            get { return _extent.StartOffset; }
        }

        /// <summary>
        /// Offset of token end in script buffer.
        /// </summary>
        public int Length
        {
            get
            {
                return _extent.EndOffset - _extent.StartOffset;
            }
        }

        /// <summary>
        /// Line number of token start.
        /// </summary>
        /// <remarks>
        /// StartLine, StartColumn, EndLine, and EndColumn are 1-based,
        /// i.e., first line has a line number 1 and first character in
        /// a line has column number 1.
        /// </remarks>
        public int StartLine { get { return _extent.StartLineNumber; } }

        /// <summary>
        /// Position of token start in start line.
        /// </summary>
        public int StartColumn { get { return _extent.StartColumnNumber; } }

        /// <summary>
        /// Line number of token end.
        /// </summary>
        public int EndLine { get { return _extent.EndLineNumber; } }

        /// <summary>
        /// Position of token end in end line.
        /// </summary>
        public int EndColumn { get { return _extent.EndColumnNumber; } }

        #endregion
    }

    /// <summary>
    /// PowerShell token types.
    /// </summary>
    public enum PSTokenType
    {
        /// <summary>
        /// Unknown token.
        /// </summary>
        /// <remarks>
        /// </remarks>
        Unknown,

        /// <summary>
        /// Command.
        /// </summary>
        /// <remarks>
        /// For example, 'get-process' in
        ///
        ///     get-process -name foo
        /// </remarks>
        Command,

        /// <summary>
        /// Command Parameter.
        /// </summary>
        /// <remarks>
        /// For example, '-name' in
        ///
        ///     get-process -name foo
        /// </remarks>
        CommandParameter,

        /// <summary>
        /// Command Argument.
        /// </summary>
        /// <remarks>
        /// For example, 'foo' in
        ///
        ///     get-process -name foo
        /// </remarks>
        CommandArgument,

        /// <summary>
        /// Number.
        /// </summary>
        /// <remarks>
        /// For example, 12 in
        ///
        ///     $a=12
        /// </remarks>
        Number,

        /// <summary>
        /// String.
        /// </summary>
        /// <remarks>
        /// For example, "12" in
        ///
        ///     $a="12"
        /// </remarks>
        String,

        /// <summary>
        /// Variable.
        /// </summary>
        /// <remarks>
        /// For example, $a in
        ///
        ///     $a="12"
        /// </remarks>
        Variable,

        /// <summary>
        /// Property name or method name.
        /// </summary>
        /// <remarks>
        /// For example, Name in
        ///
        ///     $a.Name
        /// </remarks>
        Member,

        /// <summary>
        /// Loop label.
        /// </summary>
        /// <remarks>
        /// For example, :loop in
        ///
        ///     :loop
        ///     foreach($a in $b)
        ///     {
        ///         $a
        ///     }
        /// </remarks>
        LoopLabel,

        /// <summary>
        /// Attributes.
        /// </summary>
        /// <remarks>
        /// For example, Mandatory in
        ///
        ///     param([Mandatory] $a)
        /// </remarks>
        Attribute,

        /// <summary>
        /// Types.
        /// </summary>
        /// <remarks>
        /// For example, [string] in
        ///
        ///     $a = [string] 12
        /// </remarks>
        Type,

        /// <summary>
        /// Operators.
        /// </summary>
        /// <remarks>
        /// For example, + in
        ///
        ///     $a = 1 + 2
        /// </remarks>
        Operator,

        /// <summary>
        /// Group Starter.
        /// </summary>
        /// <remarks>
        /// For example, { in
        ///
        ///     if ($a -gt 4)
        ///     {
        ///         $a++;
        ///     }
        /// </remarks>
        GroupStart,

        /// <summary>
        /// Group Ender.
        /// </summary>
        /// <remarks>
        /// For example, } in
        ///
        ///     if ($a -gt 4)
        ///     {
        ///         $a++;
        ///     }
        /// </remarks>
        GroupEnd,

        /// <summary>
        /// Keyword.
        /// </summary>
        /// <remarks>
        /// For example, if in
        ///
        ///     if ($a -gt 4)
        ///     {
        ///         $a++;
        ///     }
        /// </remarks>
        Keyword,

        /// <summary>
        /// Comment.
        /// </summary>
        /// <remarks>
        /// For example, #here in
        ///
        ///     #here
        ///     if ($a -gt 4)
        ///     {
        ///         $a++;
        ///     }
        /// </remarks>
        Comment,

        /// <summary>
        /// Statement separator. This is ';'
        /// </summary>
        /// <remarks>
        /// For example, ; in
        ///
        ///     #here
        ///     if ($a -gt 4)
        ///     {
        ///         $a++;
        ///     }
        /// </remarks>
        StatementSeparator,

        /// <summary>
        /// New line. This is '\n'
        /// </summary>
        /// <remarks>
        /// For example, \n in
        ///
        ///     #here
        ///     if ($a -gt 4)
        ///     {
        ///         $a++;
        ///     }
        /// </remarks>
        NewLine,

        /// <summary>
        /// Line continuation.
        /// </summary>
        /// <remarks>
        /// For example, ` in
        ///
        ///     get-command -name `
        ///     foo
        /// </remarks>
        LineContinuation,

        /// <summary>
        /// Position token.
        /// </summary>
        /// <remarks>
        /// Position token are bogus tokens generated for identifying a location
        /// in the script.
        /// </remarks>
        Position
    }
}
