using Api.Entities;
using Api.Helpers;
using Api.Repository;
using Newtonsoft.Json;

namespace Api.Seeds;

public static class SeedConfiguration
{
    public static async Task Configure(IServiceProvider sp)
    {
        var scope = sp.CreateScope();
        var questionsRepository = scope.ServiceProvider.GetService<IBaseRepository<Question>>();
        var questionAnswersRepository = scope.ServiceProvider.GetService<IBaseRepository<QuestionAnswer>>();
        
        if(await questionsRepository.GetCountAsync(questionsRepository.FilterDefinitionBuilder.Empty) != 0)
            return;
        
        var answersSeedData = JsonConvert.DeserializeObject<List<AnswerSeedData>>(File.ReadAllText("yap-answers.json"));
        var questionsSeedData = JsonConvert.DeserializeObject<List<QuestionSeedData>>(File.ReadAllText("yap-questions.json"));

        var answers = answersSeedData.Select(x => new QuestionAnswer()
        {
            AnswerCode = x.AnswerCode,
            QuestionCode = x.QuestionCode,
            Sure = 5
        });

        var questions = questionsSeedData.Select(x => new Question()
        {
            Answers = x.Answers,
            QuestionCode = x.QuestionCode,
            QuestionText = x.QuestionText
        });

        await questionsRepository.InsertManyAsync(questions.ToList());
        await questionAnswersRepository.InsertManyAsync(answers.ToList());
    }
    
    public class Answer
    {
        public string text { get; set; }
        public bool isRight { get; set; }
    }

    public class Root
    {
        public string question { get; set; }
        public List<Answer> answers { get; set; }
    }
    
    public class QuestionSeedData
    {
        public string QuestionText { get; set; }
        public string QuestionCode { get; set; }
        public List<string> Answers;
    }

    public class AnswerSeedData
    {
        public string AnswerCode { get; set; }
        public string QuestionCode { get; set; }
    }
}