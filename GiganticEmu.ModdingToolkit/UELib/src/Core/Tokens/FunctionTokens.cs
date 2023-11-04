﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace UELib.Core
{
    public partial class UStruct
    {
        public partial class UByteCodeDecompiler
        {
            public class EndFunctionParmsToken : Token
            {
            }

            public abstract class FunctionToken : Token
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                protected UName DeserializeFunctionName(IUnrealStream stream)
                {
                    return ReadName(stream);
                }

                protected void DeserializeCall()
                {
                    DeserializeParms();
                    Decompiler.DeserializeDebugToken();
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                private void DeserializeParms()
                {
#pragma warning disable 642
                    while (!(DeserializeNext() is EndFunctionParmsToken)) ;
#pragma warning restore 642
                }

                private static string PrecedenceToken(Token t)
                {
                    if (!(t is FunctionToken))
                        return t.Decompile();

                    // Always add ( and ) unless the conditions below are not met, in case of a VirtualFunctionCall.
                    var addParenthesis = true;
                    switch (t)
                    {
                        case NativeFunctionToken token:
                            addParenthesis = token.NativeItem.Type == FunctionType.Operator;
                            break;
                        case FinalFunctionToken token:
                            addParenthesis = token.Function.IsOperator();
                            break;
                    }

                    return addParenthesis
                        ? $"({t.Decompile()})"
                        : t.Decompile();
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                private bool NeedsSpace(string operatorName)
                {
                    return char.IsUpper(operatorName[0])
                           || char.IsLower(operatorName[0]);
                }

                protected string DecompilePreOperator(string operatorName)
                {
                    string operand = DecompileNext();
                    AssertSkipCurrentToken<EndFunctionParmsToken>();

                    // Only space out if we have a non-symbol operator name.
                    return NeedsSpace(operatorName)
                        ? $"{operatorName} {operand}"
                        : $"{operatorName}{operand}";
                }

                protected string DecompileOperator(string operatorName)
                {
                    var output =
                        $"{PrecedenceToken(NextToken())} {operatorName} {PrecedenceToken(NextToken())}";
                    AssertSkipCurrentToken<EndFunctionParmsToken>();
                    return output;
                }

                protected string DecompilePostOperator(string operatorName)
                {
                    string operand = DecompileNext();
                    AssertSkipCurrentToken<EndFunctionParmsToken>();

                    // Only space out if we have a non-symbol operator name.
                    return NeedsSpace(operatorName)
                        ? $"{operand} {operatorName}"
                        : $"{operand}{operatorName}";
                }

                protected string DecompileCall(string functionName)
                {
                    if (Decompiler._IsWithinClassContext)
                    {
                        functionName = $"static.{functionName}";

                        // Set false elsewhere as well but to be sure we set it to false here to avoid getting static calls inside the params.
                        // e.g.
                        // A1233343.DrawText(Class'BTClient_Interaction'.static.A1233332(static.Max(0, A1233328 - A1233322[A1233222].StartTime)), true);
                        Decompiler._IsWithinClassContext = false;
                    }

                    string arguments = DecompileParms();
                    var output = $"{functionName}({arguments})";
                    return output;
                }

                private string DecompileParms()
                {
                    var tokens = new List<Tuple<Token, string>>();
                    {
                    next:
                        var t = NextToken();
                        tokens.Add(Tuple.Create(t, t.Decompile()));
                        if (!(t is EndFunctionParmsToken))
                            goto next;
                    }

                    var output = new StringBuilder();
                    for (var i = 0; i < tokens.Count; ++i)
                    {
                        var t = tokens[i].Item1; // Token
                        string v = tokens[i].Item2; // Value

                        switch (t)
                        {
                            // Skipped optional parameters
                            case NoParmToken _:
                                output.Append(v);
                                break;

                            // End ")"
                            case EndFunctionParmsToken _:
                                output = new StringBuilder(output.ToString().TrimEnd(','));
                                break;

                            // Any passed values
                            default:
                                {
                                    if (i != tokens.Count - 1 && i > 0) // Skipped optional parameters
                                    {
                                        output.Append(v == string.Empty ? "," : ", ");
                                    }

                                    output.Append(v);
                                    break;
                                }
                        }
                    }

                    return output.ToString();
                }
            }

            public class FinalFunctionToken : FunctionToken
            {
                public UFunction Function;

                public override void Deserialize(IUnrealStream stream)
                {
                    if (stream.Package.Build == UnrealPackage.GameBuild.BuildName.MOHA)
                    {
                        Decompiler.AlignSize(sizeof(int));
                    }

                    Function = stream.ReadObject<UFunction>();
                    Decompiler.AlignObjectSize();

                    DeserializeCall();
                }

                public override string Decompile()
                {
                    var output = string.Empty;
                    if (Function != null)
                    {
                        // Support for non native operators.
                        if (Function.IsPost())
                        {
                            output = DecompilePreOperator(Function.FriendlyName);
                        }
                        else if (Function.IsPre())
                        {
                            output = DecompilePostOperator(Function.FriendlyName);
                        }
                        else if (Function.IsOperator())
                        {
                            output = DecompileOperator(Function.FriendlyName);
                        }
                        else
                        {
                            // Calling Super??.
                            if (Function.Name == Decompiler._Container.Name && !Decompiler._IsWithinClassContext)
                            {
                                output = "super";

                                // Check if the super call is within the super class of this functions outer(class)
                                var container = Decompiler._Container;
                                var context = (UField)container.Outer;
                                if (context?.Super == null || Function.GetOuterName() != context.Super.Name)
                                {
                                    // There's no super to call then do a recursive super call.
                                    if (container.Super == null)
                                    {
                                        output += $"({container.GetOuterName()})";
                                    }
                                    else
                                    {
                                        // Different owners, then it is a deep super call.
                                        if (Function.GetOuterName() != container.GetOuterName())
                                        {
                                            output += $"({Function.GetOuterName()})";
                                        }
                                    }
                                }

                                output += ".";
                            }

                            output += DecompileCall(Function.Name);
                        }
                    }

                    Decompiler._CanAddSemicolon = true;
                    return output;
                }
            }

            public class VirtualFunctionToken : FunctionToken
            {
                public UName FunctionName;

                public override void Deserialize(IUnrealStream stream)
                {
                    // TODO: Corrigate Version (Definitely not in MOHA, but in roboblitz(369))
                    if (stream.Version >= 178 && stream.Version < 421 /*MOHA*/)
                    {
                        byte isSuperCall = stream.ReadByte();
                        Decompiler.AlignSize(sizeof(byte));
                    }

                    if (stream.Package.Build == UnrealPackage.GameBuild.BuildName.MOHA)
                    {
                        Decompiler.AlignSize(sizeof(int));
                    }

                    FunctionName = DeserializeFunctionName(stream);
                    DeserializeCall();
                }

                public override string Decompile()
                {
                    Decompiler._CanAddSemicolon = true;
                    return DecompileCall(FunctionName);
                }
            }

            public class GlobalFunctionToken : FunctionToken
            {
                public UName FunctionName;

                public override void Deserialize(IUnrealStream stream)
                {
                    FunctionName = DeserializeFunctionName(stream);
                    DeserializeCall();
                }

                public override string Decompile()
                {
                    Decompiler._CanAddSemicolon = true;
                    return $"global.{DecompileCall(FunctionName)}";
                }
            }

            public class DelegateFunctionToken : FunctionToken
            {
                public byte? IsLocal;
                public UProperty DelegateProperty;
                public UName FunctionName;

                public override void Deserialize(IUnrealStream stream)
                {
                    // TODO: Corrigate Version
                    if (stream.Version >= 181)
                    {
                        IsLocal = stream.ReadByte();
                        Decompiler.AlignSize(sizeof(byte));
                    }

                    DelegateProperty = stream.ReadObject<UProperty>();
                    Decompiler.AlignObjectSize();

                    FunctionName = DeserializeFunctionName(stream);
                    DeserializeCall();
                }

                public override string Decompile()
                {
                    Decompiler._CanAddSemicolon = true;
                    return DecompileCall(FunctionName);
                }
            }

            public class NativeFunctionToken : FunctionToken
            {
                public NativeTableItem NativeItem;

                public override void Deserialize(IUnrealStream stream)
                {
                    DeserializeCall();
                }

                public override string Decompile()
                {
                    string output;
                    switch (NativeItem.Type)
                    {
                        case FunctionType.Function:
                            output = DecompileCall(NativeItem.Name);
                            break;

                        case FunctionType.Operator:
                            output = DecompileOperator(NativeItem.Name);
                            break;

                        case FunctionType.PostOperator:
                            output = DecompilePostOperator(NativeItem.Name);
                            break;

                        case FunctionType.PreOperator:
                            output = DecompilePreOperator(NativeItem.Name);
                            break;

                        default:
                            output = DecompileCall(NativeItem.Name);
                            break;
                    }

                    Decompiler._CanAddSemicolon = true;
                    return output;
                }
            }
        }
    }
}