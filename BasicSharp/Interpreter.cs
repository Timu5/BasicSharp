using System;
using System.Collections.Generic;

namespace BasicSharp
{
    public class Interpreter
    {
        public bool HasPrint { get; set; } = true;
        public bool HasInput { get; set; } = true;

        private Lexer lex;
        private Token prevToken;
        private Token lastToken;

        private Dictionary<string, Value> vars;
        private Dictionary<string, Marker> labels;
        private Dictionary<string, Marker> loops;

        public delegate Value BasicFunction(Interpreter interpreter, List<Value> args);
        private Dictionary<string, BasicFunction> funcs;

        private int ifcounter;

        private Marker lineMarker;

        private bool exit;

        public Interpreter(string input)
        {
            this.lex = new Lexer(input);
            this.vars = new Dictionary<string, Value>();
            this.labels = new Dictionary<string, Marker>();
            this.loops = new Dictionary<string, Marker>();
            this.funcs = new Dictionary<string, BasicFunction>();
            this.ifcounter = 0;
            BuiltInFunctions.InstallAll(this);
        }

        public Value GetVar(string name)
        {
            if (!vars.ContainsKey(name))
                throw new Exception("Variable with name " + name + " does not exist.");
            return vars[name];
        }

        public void SetVar(string name, Value val)
        {
            if (!vars.ContainsKey(name)) vars.Add(name, val);
            else vars[name] = val;
        }

        public void AddFunction(string name, BasicFunction function)
        {
            if (!funcs.ContainsKey(name)) funcs.Add(name, function);
            else funcs[name] = function;
        }

        void Error(string text)
        {
            throw new Exception(text + " at line: " + lineMarker.Line);
        }

        void Match(Token tok)
        {
            if (lastToken != tok)
                Error("Expect " + tok.ToString() + " got " + lastToken.ToString());
        }

        public void Exec()
        {
            exit = false;
            GetNextToken();
            while (!exit) Line();
        }

        Token GetNextToken()
        {
            prevToken = lastToken;
            lastToken = lex.GetToken();

            if (lastToken == Token.EOF && prevToken == Token.EOF)
                Error("Unexpected end of file");

            return lastToken;
        }

        void Line()
        {
            while (lastToken == Token.NewLine) GetNextToken();

            if (lastToken == Token.EOF)
            {
                exit = true;
                return;
            }

            lineMarker = lex.TokenMarker;
            Statment();

            if (lastToken != Token.NewLine && lastToken != Token.EOF)
                Error("Expect new line got " + lastToken.ToString());
        }

        void Statment()
        {
            Token keyword = lastToken;
            GetNextToken();
            switch (keyword)
            {
                case Token.Print: Print(); break;
                case Token.Input: Input(); break;
                case Token.Goto: Goto(); break;
                case Token.If: If(); break;
                case Token.Else: Else(); break;
                case Token.EndIf: break;
                case Token.For: For(); break;
                case Token.Next: Next(); break;
                case Token.Let: Let(); break;
                case Token.End: End(); break;
                case Token.Identifer:
                    if (lastToken == Token.Equal) Let();
                    else if (lastToken == Token.Colon) Label();
                    else goto default;
                    break;
                case Token.EOF:
                    exit = true;
                    break;
                default:
                    Error("Expect keyword got " + keyword.ToString());
                    break;
            }
            if (lastToken == Token.Colon)
            {
                GetNextToken();
                Statment();
            }
        }

        void Print()
        {
            if (!HasPrint)
                Error("Print command not allowed");

            Console.WriteLine(Expr().ToString());
        }

        void Input()
        {
            if (!HasInput)
                Error("Input command not allowed");

            while (true)
            {
                Match(Token.Identifer);

                if (!vars.ContainsKey(lex.Identifer)) vars.Add(lex.Identifer, new Value());

                string input = Console.ReadLine();
                double d;
                if (double.TryParse(input, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out d))
                    vars[lex.Identifer] = new Value(d);
                else
                    vars[lex.Identifer] = new Value(input);

                GetNextToken();
                if (lastToken != Token.Comma) break;
                GetNextToken();
            }
        }

        void Goto()
        {
            Match(Token.Identifer);
            string name = lex.Identifer;

            if (!labels.ContainsKey(name))
            {
                while (true)
                {
                    if (GetNextToken() == Token.Colon && prevToken == Token.Identifer)
                    {
                        if (!labels.ContainsKey(lex.Identifer))
                            labels.Add(lex.Identifer, lex.TokenMarker);
                        if (lex.Identifer == name)
                            break;
                    }
                    if (lastToken == Token.EOF)
                    {
                        Error("Cannot find label named " + name);
                    }
                }
            }
            lex.GoTo(labels[name]);
            lastToken = Token.NewLine;
        }

        void If()
        {
            bool result = (Expr().BinOp(new Value(0), Token.Equal).Real == 1);

            Match(Token.Then);
            GetNextToken();

            if (result)
            {
                int i = ifcounter;
                while (true)
                {
                    if (lastToken == Token.If)
                    {
                        i++;
                    }
                    else if (lastToken == Token.Else)
                    {
                        if (i == ifcounter)
                        {
                            GetNextToken();
                            return;
                        }
                    }
                    else if (lastToken == Token.EndIf)
                    {
                        if (i == ifcounter)
                        {
                            GetNextToken();
                            return;
                        }
                        i--;
                    }
                    GetNextToken();
                }
            }
        }

        void Else()
        {
            int i = ifcounter;
            while (true)
            {
                if (lastToken == Token.If)
                {
                    i++;
                }
                else if (lastToken == Token.EndIf)
                {
                    if (i == ifcounter)
                    {
                        GetNextToken();
                        return;
                    }
                    i--;
                }
                GetNextToken();
            }
        }

        void Label()
        {
            string name = lex.Identifer;
            if (!labels.ContainsKey(name)) labels.Add(name, lex.TokenMarker);

            GetNextToken();
            Match(Token.NewLine);
        }

        void End()
        {
            exit = true;
        }

        void Let()
        {
            if (lastToken != Token.Equal)
            {
                Match(Token.Identifer);
                GetNextToken();
                Match(Token.Equal);
            }

            string id = lex.Identifer;

            GetNextToken();

            SetVar(id, Expr());
        }

        void For()
        {
            Match(Token.Identifer);
            string var = lex.Identifer;

            GetNextToken();
            Match(Token.Equal);

            GetNextToken();
            Value v = Expr();

            if (loops.ContainsKey(var))
            {
                loops[var] = lineMarker;
            }
            else
            {
                SetVar(var, v);
                loops.Add(var, lineMarker);
            }

            Match(Token.To);

            GetNextToken();
            v = Expr();

            if (vars[var].BinOp(v, Token.More).Real == 1)
            {
                while (true)
                {
                    while (!(GetNextToken() == Token.Identifer && prevToken == Token.Next)) ;
                    if (lex.Identifer == var)
                    {
                        loops.Remove(var);
                        GetNextToken();
                        Match(Token.NewLine);
                        break;
                    }
                }
            }

        }

        void Next()
        {
            Match(Token.Identifer);
            string var = lex.Identifer;
            vars[var] = vars[var].BinOp(new Value(1), Token.Plus);
            lex.GoTo(new Marker(loops[var].Pointer - 1, loops[var].Line, loops[var].Column - 1));
            lastToken = Token.NewLine;
        }

        Value Expr(int min = 0)
        {
            Dictionary<Token, int> precedens = new Dictionary<Token, int>()
            {
                { Token.Or, 0 }, { Token.And, 0 },
                { Token.Equal, 1 }, { Token.NotEqual, 1 },
                { Token.Less, 1 }, { Token.More, 1 },
                { Token.LessEqual, 1 },  { Token.MoreEqual, 1 },
                { Token.Plus, 2 }, { Token.Minus, 2 },
                { Token.Asterisk, 3 }, {Token.Slash, 3 },
                { Token.Caret, 4 }
            };

            Value lhs = Primary();

            while (true)
            {
                if (lastToken < Token.Plus || lastToken > Token.And || precedens[lastToken] < min)
                    break;

                Token op = lastToken;
                int prec = precedens[lastToken];
                int assoc = 0; // 0 left, 1 right
                int nextmin = assoc == 0 ? prec : prec + 1;
                GetNextToken();
                Value rhs = Expr(nextmin);
                lhs = lhs.BinOp(rhs, op);
            }

            return lhs;
        }

        Value Primary()
        {
            Value prim = Value.Zero;

            if (lastToken == Token.Value)
            {
                prim = lex.Value;
                GetNextToken();
            }
            else if (lastToken == Token.Identifer)
            {
                if (vars.ContainsKey(lex.Identifer))
                {
                    prim = vars[lex.Identifer];
                }
                else if (funcs.ContainsKey(lex.Identifer))
                {
                    string name = lex.Identifer;
                    List<Value> args = new List<Value>();
                    GetNextToken();
                    Match(Token.LParen);

                start:
                    if (GetNextToken() != Token.RParen)
                    {
                        args.Add(Expr());
                        if (lastToken == Token.Comma)
                            goto start;
                    }

                    prim = funcs[name](null, args);
                }
                else
                {
                    Error("Undeclared variable " + lex.Identifer);
                }
                GetNextToken();
            }
            else if (lastToken == Token.LParen)
            {
                GetNextToken();
                prim = Expr();
                Match(Token.RParen);
                GetNextToken();
            }
            else if (lastToken == Token.Plus || lastToken == Token.Not)
            {
                Token op = lastToken;
                GetNextToken();
                prim = Value.Zero.BinOp(Primary(), op); // we dont realy have a unary operators
            }
            else
            {
                Error("Unexpexted token in primary!");
            }

            return prim;
        }
    }
}
