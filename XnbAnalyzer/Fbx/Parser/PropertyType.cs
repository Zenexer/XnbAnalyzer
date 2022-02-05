using System;

public enum PropertyType : byte
{
    Int16 = (byte)'Y',
    Int32 = (byte)'I',
    Int64 = (byte)'L',
    Single = (byte)'F',
    Double = (byte)'D',
    Boolean = (byte)'C',

    Int32Array = (byte)'i',
    Int64Array = (byte)'l',
    SingleArray = (byte)'f',
    DoubleArray = (byte)'d',
    BooleanArray = (byte)'b',

    String = (byte)'S',
    Binary = (byte)'R',
}
