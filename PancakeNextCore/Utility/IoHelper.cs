using GrasshopperIO;
using GrasshopperIO.DataBase;
using PancakeNextCore.Dataset;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.Utility;
internal static class IoHelper
{
    public static Value CreateFromObject(object obj)
    {
        return obj switch
        {
            bool v_bool => (Value)v_bool,
            byte v_byte => (Value)v_byte,
            int v_int => (Value)v_int,
            long v_long => (Value)v_long,
            uint v_uint => (Value)v_uint,
            ulong v_ulong => (Value)v_ulong,
            Guid v_guid => (Value)v_guid,
            BigInteger v_bigInt => (Value)v_bigInt,
            DateTime v_dateTime => (Value)v_dateTime,
            TimeSpan v_timeSpan => (Value)v_timeSpan,
            float v_float => (Value)v_float,
            double v_double => (Value)v_double,
            decimal v_decimal => (Value)v_decimal,
            Complex v_complex => (Value)v_complex,
            string v_string => (Value)v_string,
            AbsRelPaths v_absRelPaths => (Value)v_absRelPaths,
            Version v_version => (Value)v_version,
            byte[] v_byteArray => (Value)v_byteArray,
            bool[] v_boolArray => (Value)v_boolArray,
            int[] v_intArray => (Value)v_intArray,
            long[] v_longArray => (Value)v_longArray,
            uint[] v_uintArray => (Value)v_uintArray,
            ulong[] v_ulongArray => (Value)v_ulongArray,
            Guid[] v_guidArray => (Value)v_guidArray,
            BigInteger[] v_bigIntArray => (Value)v_bigIntArray,
            DateTime[] v_dateTimeArray => (Value)v_dateTimeArray,
            TimeSpan[] v_timeSpanArray => (Value)v_timeSpanArray,
            float[] v_floatArray => (Value)v_floatArray,
            double[] v_doubleArray => (Value)v_doubleArray,
            decimal[] v_decimalArray => (Value)v_decimalArray,
            Complex[] v_complexArray => (Value)v_complexArray,
            string[] v_stringArray => (Value)v_stringArray,
            AbsRelPaths[] v_absRelPathsArray => (Value)v_absRelPathsArray,
            Version[] v_versionArray => (Value)v_versionArray,
            ISerializable => throw new ArgumentException("ISerializable objects are disallowed due to security concerns.", nameof(obj)),
            IStorable => throw new ArgumentException("Storable items should be saved by IWriter.Storable. They cannot be saved by Value.", nameof(obj)),
            _ => throw new ArgumentException("Unsupported type", nameof(obj)),
        };
    }
}
