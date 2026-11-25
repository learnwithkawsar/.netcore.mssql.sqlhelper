using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Core.MSSQL.SqlHelper
{
    /// <summary>
    /// Validates and quotes SQL Server identifiers (table / column names) so they can be
    /// safely embedded in dynamically built T-SQL.
    /// </summary>
    internal static class SqlIdentifier
    {
        // A regular (unbracketed) identifier part: letter or underscore, then letters, digits, _, @, $ or #.
        private static readonly Regex RegularPart = new Regex(@"^[\p{L}_][\p{L}\p{Nd}_@$#]*$", RegexOptions.Compiled);

        /// <summary>
        /// Turns a possibly multi-part name (e.g. <c>dbo.Users</c> or <c>[dbo].[My Table]</c>)
        /// into a fully bracket-quoted name. Throws <see cref="ArgumentException"/> if the
        /// name is not a valid identifier.
        /// </summary>
        public static string Quote(string name, string paramName)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Identifier must not be empty.", paramName);
            }

            var parts = Split(name.Trim(), paramName);
            if (parts.Length > 4)
            {
                throw new ArgumentException($"'{name}' has too many name parts.", paramName);
            }

            var sb = new StringBuilder();
            for (int i = 0; i < parts.Length; i++)
            {
                if (i > 0) sb.Append('.');
                sb.Append('[').Append(parts[i].Replace("]", "]]")).Append(']');
            }
            return sb.ToString();
        }

        private static string[] Split(string name, string paramName)
        {
            var result = new System.Collections.Generic.List<string>();
            int i = 0;
            while (i < name.Length)
            {
                string part;
                if (name[i] == '[')
                {
                    // Bracketed part: read until a ']' that is not doubled.
                    var sb = new StringBuilder();
                    i++;
                    bool closed = false;
                    while (i < name.Length)
                    {
                        if (name[i] == ']')
                        {
                            if (i + 1 < name.Length && name[i + 1] == ']')
                            {
                                sb.Append(']');
                                i += 2;
                                continue;
                            }
                            closed = true;
                            i++;
                            break;
                        }
                        sb.Append(name[i]);
                        i++;
                    }
                    if (!closed || sb.Length == 0 || sb.Length > 128)
                    {
                        throw new ArgumentException($"'{name}' is not a valid SQL identifier.", paramName);
                    }
                    part = sb.ToString();
                }
                else
                {
                    int end = name.IndexOf('.', i);
                    if (end < 0) end = name.Length;
                    part = name.Substring(i, end - i);
                    if (part.Length > 128 || !RegularPart.IsMatch(part))
                    {
                        throw new ArgumentException($"'{name}' is not a valid SQL identifier.", paramName);
                    }
                    i = end;
                }

                result.Add(part);

                if (i < name.Length)
                {
                    if (name[i] != '.' || i == name.Length - 1)
                    {
                        throw new ArgumentException($"'{name}' is not a valid SQL identifier.", paramName);
                    }
                    i++;
                }
            }
            return result.ToArray();
        }
    }
}
