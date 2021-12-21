using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using XnbAnalyzer.Xnb.Content;
using XnbAnalyzer.Xnb.Reading;

namespace XnbAnalyzer.Xnb
{
    public class TypeDefinition
    {
        public string Name { get; }
        public ImmutableArray<TypeDefinition> TypeParameters { get; }
        public int Version { get; }
        public bool IsGeneric => TypeParameters.Length > 0;
        public bool IsReflective => Name == "Microsoft.Xna.Framework.Content.ReflectiveReader`1";

        public TypeDefinition Resolved
        {
            get
            {
                var x = this;

                while (x.IsReflective)
                {
                    x = TypeParameters[0];
                }

                return x;
            }
        }

        // Example: Microsoft.Xna.Framework.Content.ArrayReader`1[[Microsoft.Xna.Framework.Graphics.Texture]]

        public TypeDefinition(string fullName, int version)
            : this(fullName.AsSpan(), version, out var _) { }

        public override string ToString()
        {
            if (!IsGeneric)
            {
                return Name;
            }

            return $"{Name}[[{string.Join("],[", TypeParameters)}]]";
        }

        private enum State
        {
            Invalid = 0,
            Name,
            TypeParamCount,
            TypeParamList,
            TypeParamListDelim,
            AssemblyInfo,
            End,
        }

        private TypeDefinition(ReadOnlySpan<char> fullName, int version, out int consumed)
        {
            var state = State.Name;
            var typeParamStart = 0;
            var typeParamCount = 0;
            var genericTypeParams = new List<TypeDefinition>();
            int i;

            for (i = 0; i < fullName.Length && state != State.End; i++)
            {
                var c = fullName[i];

                switch (state)
                {
                    case State.Name:
                        switch (c)
                        {
                            case '`':
                                typeParamStart = i + 1;
                                state = State.TypeParamCount;
                                break;

                            case ',':
                                Name = fullName[..i].ToString();
                                state = State.AssemblyInfo;
                                break;

                            case ']':
                                Name = fullName[..i].ToString();
                                state = State.End;
                                break;

                            case '[':
                                throw new XnbFormatException("Invalid type name: array type spec notation isn't supported by XNB");

                            case '&':
                                throw new XnbFormatException("Invalid type name: reference type spec notation isn't supported by XNB");
                        }
                        break;

                    case State.TypeParamCount:
                        switch (c)
                        {
                            case '0':
                            case '1':
                            case '2':
                            case '3':
                            case '4':
                            case '5':
                            case '6':
                            case '7':
                            case '8':
                            case '9':
                                break;

                            case '[':
                                if (typeParamStart == i)
                                {
                                    throw new XnbFormatException("Invalid type name: missing number of generic type parameters");
                                }

                                state = State.TypeParamList;
                                Name = fullName[..i].ToString();
                                typeParamCount = int.Parse(fullName[typeParamStart..i]);
                                break;

                            default:
                                throw new XnbFormatException("Invalid type name: expected generic type parameter list");
                        }
                        break;

                    case State.TypeParamList:
                        switch (c)
                        {
                            case '[':
                                genericTypeParams.Add(new TypeDefinition(fullName[(i + 1)..], 0, out var innerConsumed));
                                i += innerConsumed;  // We're currently -1 from the start of the inner span.  This cancels out the i++ on the next loop without any extra math.
                                state = State.TypeParamListDelim;
                                break;

                            default:
                                throw new XnbFormatException($"Invalid type name: expected [, got {c}");
                        }
                        break;

                    case State.TypeParamListDelim:
                        switch (c)
                        {
                            case ',':
                                state = State.TypeParamList;
                                break;

                            case ']':
                                state = State.AssemblyInfo;
                                break;

                            default:
                                throw new XnbFormatException($"Invalid type name: expected , or ], got {c}");
                        }
                        break;

                    case State.AssemblyInfo:
                        switch (c)
                        {
                            case ']':
                                state = State.End;
                                break;

                            // Not worth parsing the rest
                        }
                        break;
                }
            }

            switch (state)
            {
                case State.Name:
                    Name = fullName.ToString();
                    break;

                case State.AssemblyInfo:
                    break;

                case State.End:
                    // i wasn't incremented, 
                    break;

                case State.TypeParamCount:
                    throw new XnbFormatException("Invalid type name: expected generic type parameter count");

                case State.TypeParamList:
                    throw new XnbFormatException("Invalid type name: unterminated generic type parameter list or trailing comma");

                case State.TypeParamListDelim:
                    throw new XnbFormatException("Invalid type name: unterminated generic type parameter list");
            }

            if (typeParamCount != genericTypeParams.Count)
            {
                throw new XnbFormatException($"Invalid type name: expected {typeParamCount} generic type parameter(s), got {genericTypeParams.Count}");
            }

            consumed = i;
            TypeParameters = genericTypeParams.ToImmutableArray();

            if (Name is null)
            {
                throw new Exception("Logic error while parsing XNB type name");
            }
        }
    }
}
