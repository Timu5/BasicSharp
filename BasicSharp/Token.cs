
namespace BasicSharp
{
    public enum Token
    {
        Unkown,

        Identifer,
        Value,

        //Keywords
        Print,
        If,
        Then,
        Else,
        For,
        To,
        Next,
        Goto,
        Input,
        Let,
        Gosub,
        Return,
        Rem,
        End,

        NewLine,
        Colon,
        Semicolon,
        Comma,

        Plus,
        Minus,
        Slash,
        Asterisk,
        Caret,
        LParen,
        RParen,
        Equal,
        Less,
        More,
        NotEqual,
        LessEqual,
        MoreEqual,
        Or,
        And,
        Not,

        EOF = -1   //End Of File
    }
}
