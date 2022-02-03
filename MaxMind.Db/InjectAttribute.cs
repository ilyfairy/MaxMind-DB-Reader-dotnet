﻿#region

using System;

#endregion

namespace MaxMind.Db
{
    /// <summary>
    ///     Instruct <c>Reader</c> to map database key to constructor parameter.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class InjectAttribute : Attribute
    {
        /// <summary>
        ///     The name to use for the property.
        /// </summary>
        public string ParameterName { get; }

        /// <summary>
        ///     Create a new instance of <c>InjectAttribute</c>.
        /// </summary>
        /// <param name="parameterName"></param>
        public InjectAttribute(string parameterName)
        {
            ParameterName = parameterName;
        }
    }
}