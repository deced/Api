namespace Api.Models;

public class SubmitQuestionModel
{
    public string Question { get; set; }
    public string SelectedAnswer { get; set; }
    public List<string> Answers { get; set; }
}