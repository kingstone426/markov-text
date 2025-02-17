namespace MarkovText;

public class SentenceOverflowException : Exception
{
    public SentenceOverflowException(string message) : base(message) { }
}
