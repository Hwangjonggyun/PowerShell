// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.PowerShell.Commands
{
    /// <summary>
    /// WebCmdletElementCollection for elements in html web responses.
    /// </summary>
    public class WebCmdletElementCollection : ReadOnlyCollection<PSObject>
    {
        internal WebCmdletElementCollection(IList<PSObject> list)
            : base(list)
        {
        }

        /// <summary>
        /// Finds the element with name or id.
        /// </summary>
        /// <param name="nameOrId"></param>
        /// <returns>Found element as PSObject.</returns>
        public PSObject Find(string nameOrId)
        {
            // try Id first
            PSObject result = FindById(nameOrId) ?? FindByName(nameOrId);

            return (result);
        }

        /// <summary>
        /// Finds the element by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Found element as PSObject.</returns>
        public PSObject FindById(string id)
        {
            return Find(id, true);
        }

        /// <summary>
        /// Finds the element by name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>Found element as PSObject.</returns>
        public PSObject FindByName(string name)
        {
            return Find(name, false);
        }

        private PSObject Find(string nameOrId, bool findById)
        {
            foreach (PSObject candidate in this)
            {
                var namePropInfo = candidate.Properties[(findById ? "id" : "name")];
                if (namePropInfo != null && (string)namePropInfo.Value == nameOrId)
                {
                    return candidate;
                }
            }

            return null;
        }
    }
}
