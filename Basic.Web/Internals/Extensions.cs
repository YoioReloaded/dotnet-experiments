#region Includes
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
#endregion

namespace Basic.Web.Internals.Extensions
{
    ///<summary>Use to determine the sorting for object.Entries</summary>
    internal enum EntriesSorting
    {
        Ascending = 1,
        Desceding = -1,
        None = 0
    }

    ///<summary>Extensions for all object interfaces</summary>
    internal static class ObjectExtensions
    {
        ///<summary>Given the object returns the dictionary of key value pairs for readable properties</summary>
        public static IEnumerable<(string Key, object Value, int Index)> Entries<T>(
            this T obj,
            EntriesSorting sorting=EntriesSorting.None
        )
        {
            Type _t = obj.GetType();
            return obj.HasInterface<System.Collections.IEnumerable, T>()
            ? ((IEnumerable<object>)obj).Select(
                (value, index) => ($"index", value, index)
            )
            : (sorting switch {
                EntriesSorting.None
                => _t.GetProperties().AsEnumerable(),
                EntriesSorting.Ascending
                => _t.GetProperties().OrderBy( p => p.Name ),
                EntriesSorting.Desceding
                => _t.GetProperties().OrderByDescending( p => p.Name ),
                _
                => throw new ArgumentOutOfRangeException(nameof(sorting), $"Unexpected sorting value: {sorting}")   
            }).Select(
                (PropertyInfo prop, int index) => 
                {
                    try { return (prop.Name, prop.CanRead ? prop.GetValue(obj) : "*non-readable*", index); }
                    catch (TargetParameterCountException ex) {
                        PropertyInfo objCount = _t.GetProperty("Count"), objKeys = _t.GetProperty("Keys");
                        return ( prop.Name, 
                            (new { 
                                CountCanRead = objCount?.CanRead,
                                CountPropertyInfoType = objCount?.GetType(),
                                CountPropertyType = objCount?.PropertyType,
                                KeysCanRead = objKeys?.CanRead,
                                KeysPropertyInfoType = objKeys?.GetType(),
                                KeysPropertyType = objKeys?.PropertyType,
                            }) switch {
                                { CountCanRead: var CCR, CountPropertyInfoType: var CPIT,  CountPropertyType: var CPT,
                                KeysCanRead: var KCR, KeysPropertyInfoType: var KPIT, KeysPropertyType: var KPT }
                                when ("System.Int32" == CPT.FullName
                                    && KPT is not null
                                    && KPT.GetInterfaces().Contains(typeof(System.Collections.IEnumerable))
                                    && "System.Reflection.RuntimePropertyInfo" == CPIT.FullName
                                    && "System.Reflection.RuntimePropertyInfo" == KPIT.FullName
                                    && true == CCR && true == KCR)
                                => 0 < (Int32)objCount.GetValue(obj)
                                    ? ((IEnumerable<object>)objKeys.GetValue(obj)).Aggregate(new object[] {}, (acc, cur) => acc.Concat(new object[] { cur }).ToArray() )
                                    : Enumerable.Empty<object>(), // Empty collections value are represented by an empty enumerable
                                _
                                => "*error: unconvertible runtime property*\n{ex}"
                            },
                            index );
                    }
                    catch (TargetInvocationException ex) { return (prop.Name, $"*error*\n{ex}", index); }
                }
            );
        }

        ///<summary>Returns true if the object implements the specified interface type.</summary>
        public static bool HasInterface<TyInterface, T>(this T obj) where TyInterface : class => typeof(T).GetInterfaces().Contains(typeof(TyInterface));

    }

    ///<summary>Extensions for all dictionary interfaces</summary>
    internal static class DictionaryExtensions
    {
        ///<summary>Given the dictionary applies ToString method to each key value pair or the given join expression</summary>
        public static string ToStringJoin<TKey,TValue>(
            this IDictionary<TKey,TValue> dict,
            Func<KeyValuePair<TKey,TValue>, string> joinExpression = null
        )
        {
            if (dict is not null) {
                var _f = joinExpression ?? ((kv) => $"\n{kv.Key.ToString()}:\t{kv.Value?.ToString()}");
                
                return dict.Aggregate(
                    "",
                    (acc, x) => $"{acc}{_f(x)}"
                );
            }
            else throw new ArgumentNullException(nameof(dict), $"Expecting a valid reference, but got {null} instead");
        }
    }

    ///<summary>Extensions for all HttpRequest interfaces</summary>
    internal static class HttpRequestExtensions
    {
        ///<summary>Defines a recursive function which takes exactly one argument.</summary>
        private delegate TReturn Recursive<Tin,TReturn>(Recursive<Tin,TReturn> func, Tin arg0);

        ///<summary>Custom string serialization for HttpRequests</summary>
        public static string Serialize(
            this HttpRequest request,
            int depth = 4
        )
        {
            int recCounter = 0;
            Recursive<KeyValuePair<string, object>, string> recursiveJoin = (f, kv) => {
                recCounter += 1;
                var _ret = (depth >= recCounter)
                    ? $"\n{new String('>', recCounter-1)} [{kv.Value?.GetType().Name ?? "null"}] \"{kv.Key}\":\t" + kv.Value?.GetType().FullName switch {
                        ("System.String" or "System.Char")
                            => $"\"{kv.Value}\"",
                        ("System.Float" or "System.Double" or "System.Decimal"
                        or "System.Int64" or "System.Int32" or "System.Int16" or "System.UInt64" or "System.UInt32" or "System.UInt16" or "System.BigInteger"
                        or "System.Byte" or "System.SByte")
                            => kv.Value,
                        ("System.Boolean")
                            => (bool)kv.Value ? "true" : "false",
                        _
                            => (kv.Value is not null)
                                ? (kv.Value.GetType().IsArray)
                                    ? $"[" + ((object[])kv.Value).Entries(EntriesSorting.Ascending).ToDictionary(
                                        (entry) => entry.Key, (entry) => entry.Value
                                    ).ToStringJoin(joinExpression: (v) => f(f,v) ) + $"\n{new String('>', recCounter-1)} ]"
                                    : kv.Value.Entries(EntriesSorting.Ascending).ToDictionary(
                                        (entry) => entry.Key, (entry) => entry.Value
                                    ).ToStringJoin(joinExpression: (v) => f(f,v) )
                                : "null",
                    }
                    : $"\n\"{kv.Key}\":{kv.Value?.ToString()}";
                recCounter -= 1;
                return _ret;
            };

            // Compile shallow JSON-like request serialization (without using the specific assembly)
            return request.Entries(EntriesSorting.Ascending).ToDictionary(
                (entry) => entry.Key, (entry) => entry.Value
            ).ToStringJoin(joinExpression: (v) => recursiveJoin(recursiveJoin, v));
        }
    }
}