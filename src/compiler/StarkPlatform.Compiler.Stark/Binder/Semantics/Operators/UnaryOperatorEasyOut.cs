﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Diagnostics;
using StarkPlatform.Compiler.Stark.Symbols;
using StarkPlatform.Compiler.Stark.Syntax;
using StarkPlatform.Compiler.Text;

namespace StarkPlatform.Compiler.Stark
{
    internal sealed partial class OverloadResolution
    {
        internal static class UnopEasyOut
        {
            private const UnaryOperatorKind ERR = UnaryOperatorKind.Error;

            private const UnaryOperatorKind BOL = UnaryOperatorKind.Bool;
            private const UnaryOperatorKind CHR = UnaryOperatorKind.Rune;
            private const UnaryOperatorKind I08 = UnaryOperatorKind.Int8;
            private const UnaryOperatorKind U08 = UnaryOperatorKind.UInt8;
            private const UnaryOperatorKind I16 = UnaryOperatorKind.Int16;
            private const UnaryOperatorKind U16 = UnaryOperatorKind.UShort;
            private const UnaryOperatorKind I32 = UnaryOperatorKind.Int32;
            private const UnaryOperatorKind U32 = UnaryOperatorKind.UInt32;
            private const UnaryOperatorKind I64 = UnaryOperatorKind.Int64;
            private const UnaryOperatorKind U64 = UnaryOperatorKind.UInt64;
            private const UnaryOperatorKind R32 = UnaryOperatorKind.Float32;
            private const UnaryOperatorKind R64 = UnaryOperatorKind.Float64;
            private const UnaryOperatorKind INT = UnaryOperatorKind.Int;
            private const UnaryOperatorKind UNT = UnaryOperatorKind.UInt;
            private const UnaryOperatorKind LBOL = UnaryOperatorKind.Lifted | UnaryOperatorKind.Bool;
            private const UnaryOperatorKind LCHR = UnaryOperatorKind.Lifted | UnaryOperatorKind.Rune;
            private const UnaryOperatorKind LI08 = UnaryOperatorKind.Lifted | UnaryOperatorKind.Int8;
            private const UnaryOperatorKind LU08 = UnaryOperatorKind.Lifted | UnaryOperatorKind.UInt8;
            private const UnaryOperatorKind LI16 = UnaryOperatorKind.Lifted | UnaryOperatorKind.Int16;
            private const UnaryOperatorKind LU16 = UnaryOperatorKind.Lifted | UnaryOperatorKind.UShort;
            private const UnaryOperatorKind LI32 = UnaryOperatorKind.Lifted | UnaryOperatorKind.Int32;
            private const UnaryOperatorKind LU32 = UnaryOperatorKind.Lifted | UnaryOperatorKind.UInt32;
            private const UnaryOperatorKind LI64 = UnaryOperatorKind.Lifted | UnaryOperatorKind.Int64;
            private const UnaryOperatorKind LU64 = UnaryOperatorKind.Lifted | UnaryOperatorKind.UInt64;
            private const UnaryOperatorKind LR32 = UnaryOperatorKind.Lifted | UnaryOperatorKind.Float32;
            private const UnaryOperatorKind LR64 = UnaryOperatorKind.Lifted | UnaryOperatorKind.Float64;
            private const UnaryOperatorKind LINT = UnaryOperatorKind.Lifted | UnaryOperatorKind.Int;
            private const UnaryOperatorKind LUNT = UnaryOperatorKind.Lifted | UnaryOperatorKind.UInt;


            private static readonly UnaryOperatorKind[] s_increment =
                //obj  str  bool chr   i08   i16   i32   i64   u08   u16   u32   u64   r32   r64   int   uint
                { ERR, ERR, ERR, CHR,  I08,  I16,  I32,  I64,  U08,  U16,  U32,  U64,  R32,  R64,  INT,  UNT,
               /* lifted */ ERR, LCHR, LI08, LI16, LI32, LI64, LU08, LU16, LU32, LU64, LR32, LR64, LINT, LUNT };

            private static readonly UnaryOperatorKind[] s_plus =
                //obj  str  bool chr   i08   i16   i32   i64   u08   u16   u32   u64   r32   r64   int   uint
                { ERR, ERR, ERR, I32,  I32,  I32,  I32,  I64,  I32,  I32,  U32,  U64,  R32,  R64,  INT,  UNT,
               /* lifted */ ERR, LI32, LI32, LI32, LI32, LI64, LI32, LI32, LU32, LU64, LR32, LR64, LINT, LUNT };

            private static readonly UnaryOperatorKind[] s_minus =
                //obj  str  bool chr   i08   i16   i32   i64   u08   u16   u32   u64   r32   r64   int   uint
                { ERR, ERR, ERR, I32,  I32,  I32,  I32,  I64,  I32,  I32,  I64,  ERR,  R32,  R64,  INT,  UNT,
               /* lifted */ ERR, LI32, LI32, LI32, LI32, LI64, LI32, LI32, LI64, ERR,  LR32, LR64, LINT, LUNT };

            private static readonly UnaryOperatorKind[] s_logicalNegation =
                //obj  str  bool chr   i08   i16   i32   i64   u08   u16   u32   u64   r32   r64   int   uint
                { ERR, ERR, BOL,  ERR, ERR,  ERR,  ERR,  ERR,  ERR,  ERR,  ERR,  ERR,  ERR,  ERR,  ERR,  ERR,
               /* lifted */ LBOL, ERR, ERR,  ERR,  ERR,  ERR,  ERR,  ERR,  ERR,  ERR,  ERR,  ERR,  ERR,  ERR };

            private static readonly UnaryOperatorKind[] s_bitwiseComplement =
                //obj  str  bool chr   i08   i16   i32   i64   u08   u16   u32   u64   r32   r64    int   uint
                { ERR, ERR, ERR, I32,  I32,  I32,  I32,  I64,  I32,  I32,  U32,  U64,  ERR,  ERR,   INT,  UNT,
               /* lifted */ ERR, LI32, LI32, LI32, LI32, LI64, LI32, LI32, LU32, LU64, ERR,  ERR,  LINT, LUNT };

            private static readonly UnaryOperatorKind[][] s_opkind =
            {
                /* ++ */  s_increment,
                /* -- */  s_increment,
                /* ++ */  s_increment,
                /* -- */  s_increment,
                /* +  */  s_plus,
                /* -  */  s_minus,
                /* !  */  s_logicalNegation,
                /* ~  */  s_bitwiseComplement
            };

            // UNDONE: This code is repeated in a bunch of places.
            private static int? TypeToIndex(TypeSymbol type)
            {
                switch (type.GetSpecialTypeSafe())
                {
                    case SpecialType.System_Object: return 0;
                    case SpecialType.System_String: return 1;
                    case SpecialType.System_Boolean: return 2;
                    case SpecialType.System_Rune: return 3;
                    case SpecialType.System_Int8: return 4;
                    case SpecialType.System_Int16: return 5;
                    case SpecialType.System_Int32: return 6;
                    case SpecialType.System_Int64: return 7;
                    case SpecialType.System_UInt8: return 8;
                    case SpecialType.System_UInt16: return 9;
                    case SpecialType.System_UInt32: return 10;
                    case SpecialType.System_UInt64: return 11;
                    case SpecialType.System_Float32: return 12;
                    case SpecialType.System_Float64: return 13;
                    case SpecialType.System_Int: return 14;
                    case SpecialType.System_UInt: return 15;

                    case SpecialType.None:
                        if ((object)type != null && type.IsNullableType())
                        {
                            TypeSymbol underlyingType = type.GetNullableUnderlyingType();

                            switch (underlyingType.GetSpecialTypeSafe())
                            {
                                case SpecialType.System_Boolean: return 17;
                                case SpecialType.System_Rune: return 18;
                                case SpecialType.System_Int8: return 19;
                                case SpecialType.System_Int16: return 20;
                                case SpecialType.System_Int32: return 21;
                                case SpecialType.System_Int64: return 22;
                                case SpecialType.System_UInt8: return 23;
                                case SpecialType.System_UInt16: return 24;
                                case SpecialType.System_UInt32: return 25;
                                case SpecialType.System_UInt64: return 26;
                                case SpecialType.System_Float32: return 27;
                                case SpecialType.System_Float64: return 28;
                                case SpecialType.System_Int: return 29;
                                case SpecialType.System_UInt: return 30;
                            }
                        }

                        // fall through
                        goto default;

                    default: return null;
                }
            }

            public static UnaryOperatorKind OpKind(UnaryOperatorKind kind, TypeSymbol operand)
            {
                int? index = TypeToIndex(operand);
                if (index == null)
                {
                    return UnaryOperatorKind.Error;
                }
                int kindIndex = kind.OperatorIndex();
                var result = (kindIndex >= s_opkind.Length) ? UnaryOperatorKind.Error : s_opkind[kindIndex][index.Value];
                return result == UnaryOperatorKind.Error ? result : result | kind;
            }
        }

        private void UnaryOperatorEasyOut(UnaryOperatorKind kind, BoundExpression operand, UnaryOperatorOverloadResolutionResult result)
        {
            var operandType = operand.Type;
            if ((object)operandType == null)
            {
                return;
            }

            var easyOut = UnopEasyOut.OpKind(kind, operandType);

            if (easyOut == UnaryOperatorKind.Error)
            {
                return;
            }

            UnaryOperatorSignature signature = this.Compilation.builtInOperators.GetSignature(easyOut);

            Conversion? conversion = Conversions.FastClassifyConversion(operandType, signature.OperandType);

            Debug.Assert(conversion.HasValue && conversion.Value.IsImplicit);

            result.Results.Add(UnaryOperatorAnalysisResult.Applicable(signature, conversion.Value));
        }
    }
}
