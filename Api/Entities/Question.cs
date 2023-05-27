namespace Api.Entities;

[BsonCollection("questions")]
public class Question : DocumentBase
{
    public string QuestionText { get; set; }
    public string QuestionCode { get; set; }
    public List<string> Answers;

    public Question()
    {
        Answers = new List<string>();
    }
}