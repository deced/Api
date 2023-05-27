namespace Api.Entities;

[BsonCollection("question_answers")]
public class QuestionAnswer : DocumentBase
{
    public string QuestionCode { get; set; }
    public string AnswerCode { get; set; }
    public int Sure { get; set; } // 10 is max
}