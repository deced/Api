namespace Api.Models;

public class GetAnswerRequest
{
    public string Question { get; set; }
    public List<string> Answers { get; set; }
}