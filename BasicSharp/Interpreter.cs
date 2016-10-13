using System;
using System.Collections.Generic;

namespace BasicSharp
{
    public class Interpreter
    {
        private Lexer lex;
        private Token prevToken;
        private Token lastToken;

        private Dictionary<string, Value> vars;
        private Dictionary<string, Marker> labels;
        private Dictionary<string, Marker> loops;

		private int ifcounter;

        private Stack<Value> stack;

        private Marker lineMarker;

        private bool exit;

        public Interpreter(string input)
        {
            this.lex = new Lexer(input);
            this.vars = new Dictionary<string, Value>();
            this.labels = new Dictionary<string, Marker>();
            this.loops = new Dictionary<string, Marker>();
            this.stack = new Stack<Value>();
			this.ifcounter = 0;
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
            if(lastToken == Token.Colon)
            {
                GetNextToken();
                Statment();
            }
        }

        void Print()
        {
            Expr();
            Console.Write(stack.Pop().ToString());
            GetNextToken();
        }

        void Input()
        {
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
            Expr();
            bool result = (stack.Pop().BinOp(new Value(0), Token.Equal).Real == 1);

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
                        if(i == ifcounter)
                        {
                            GetNextToken();
                            return;
                        }
                        i--;
                    }
                    GetNextToken ();
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
                    if(i == ifcounter)
                    {
                        GetNextToken();
                        return;
                    }
                    i--;
                }
				GetNextToken ();
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
            Expr();

            SetVar(id, stack.Pop());
        }

        void For()
        {
            Match(Token.Identifer);
            string var = lex.Identifer;

            GetNextToken();
            Match(Token.Equal);

            GetNextToken();
            Expr();

            if (loops.ContainsKey(var))
            {
                loops[var] = lineMarker;
            }
            else
            {
                SetVar(var, stack.Pop());
                loops.Add(var, lineMarker);
            }

            Match(Token.To);

            GetNextToken();
            Expr();

            Value val = stack.Pop();
            
            if (vars[var].BinOp(val, Token.More).Real == 1)
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

        void Expr()
        {
            Dictionary<Token, int> prec = new Dictionary<Token, int>() 
            {
                { Token.LParen, -1 }, { Token.RParen, -1 },
                { Token.Or, 0 }, { Token.And, 0 },
                { Token.Equal, 1 }, { Token.NotEqual, 1 },       
                { Token.Less, 1 }, { Token.More, 1 }, { Token.LessEqual, 1 },  { Token.MoreEqual, 1 },
                { Token.Plus, 2 }, { Token.Minus, 2 },
                { Token.Asterisk, 3 }, {Token.Slash, 3 },
                { Token.Caret, 4 }
            };

            Stack<Token> operators = new Stack<Token>();

            int i = 0;
            while (true)
            {
                if (lastToken == Token.Value)
                {
                    stack.Push(lex.Value);
                }
                else if (lastToken == Token.Identifer)
                {
                    if (!this.vars.ContainsKey(lex.Identifer))
                        Error("Undeclared variable " + lex.Identifer);
                    stack.Push(vars[lex.Identifer]);
                }
                else if (lastToken == Token.LParen)
                {
                    operators.Push(Token.LParen);
                }
                else if (lastToken == Token.RParen)
                {
                    while (operators.Count > 0)
                    {
                        Token t = operators.Pop();
                        if (t == Token.LParen)
                            break;
                        else
                            Operation(t);
                    }
                }
                else if (lastToken >= Token.Plus && lastToken <= Token.Not)
                {
                    if ((lastToken == Token.Minus || lastToken == Token.Minus) && (i == 0 || prevToken == Token.LParen))
                    {
                        stack.Push(new Value(0));
                        operators.Push(lastToken);
                    }
                    else
                    {
                        while (operators.Count > 0 && prec[lastToken] <= prec[operators.Peek()])
                            Operation(operators.Pop());
                        operators.Push(lastToken);
                    }
                }
                else
                {
                    if (i == 0)
                        Error("Empty expression");
                    break;
                }
                i++;
                GetNextToken();
            }

            while (operators.Count > 0)
                Operation(operators.Pop());
        }

        void Operation(Token token)
        {
            Value b = stack.Pop();
            Value a = stack.Pop();
            Value result = a.BinOp(b, token);
            stack.Push(result);
        }
    }
}
