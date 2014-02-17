using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Griffin
{
    ///// <summary>
    ///// Extension method for <c>object</c> which makes a shallow copy representation as a dictionary.
    ///// </summary>
    //public static class ObjectToDictionaryExtension
    //{
    //    /// <summary>
    //    /// Object that should be converted to a dictionary
    //    /// </summary>
    //    /// <param name="instance">Instance</param>
    //    /// <returns>Dictionary with properties as keys</returns>
    //    public static IDictionary<string, object> ToDictionary(this object instance)
    //    {
    //        if (instance == null)
    //            throw new ArgumentNullException("instance");

    //        var result = new Dictionary<string, object>();
    //        foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(instance))
    //        {
    //            var value = descriptor.GetValue(instance);
    //            result.Add(descriptor.Name, value);
    //        }
    //        return result;

    //    }

    //    public static string ToJSON(this object instance)
    //    {
    //        var formatter = new JsonFormatter();
    //        return formatter.ToJSON(instance);
    //    }
    //}
}