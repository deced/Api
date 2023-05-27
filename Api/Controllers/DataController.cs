using Api.Entities;
using Api.Helpers;
using Api.Models;
using Api.Repository;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class DataController : Controller
{
    private readonly IBaseRepository<Question> _questionsRepository;
    private readonly IBaseRepository<QuestionAnswer> _questionAnswersRepository;

    public DataController(IBaseRepository<Question> questionsRepository,
        IBaseRepository<QuestionAnswer> questionAnswerRepository)
    {
        _questionsRepository = questionsRepository;
        _questionAnswersRepository = questionAnswerRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAnswer([FromQuery]string question)
    {
        var code = FormatHelper.ConvertToCode(question);

        var questionAnswer = await _questionAnswersRepository.FindOneAsync(x => x.QuestionCode == code);

        return Json(new AnswerResponse()
        {
            Answer = questionAnswer?.AnswerCode
        });
    }

    [HttpPost]
    public async Task<IActionResult> SubmitTest([FromBody]SubmitTestModel model)
    {
        var questions = model.Questions.Select(x => new Question()
        {
            QuestionText = x.Question,
            QuestionCode = FormatHelper.ConvertToCode(x.Question),
            Answers = x.Answers
        });

        var questionAnswers = model.Questions.Select(x => new QuestionAnswer()
        {
            Sure = model.Mark == 20 ? 10 : 5,
            QuestionCode = FormatHelper.ConvertToCode(x.Question),
            AnswerCode = FormatHelper.ConvertToCode(x.SelectedAnswer)
        });

        foreach (var question in questions)
        {
            var existingQuestion =
                await _questionsRepository.FindOneAsync(x => x.QuestionCode == question.QuestionCode);
            if(existingQuestion != null)
                continue;

            await _questionsRepository.InsertOneAsync(question);
        }
        
        foreach (var questionAnswer in questionAnswers)
        {
            var existingQuestionAnswer =
                await _questionAnswersRepository.FindOneAsync(x => x.QuestionCode == questionAnswer.QuestionCode);
            if(existingQuestionAnswer != null)
                continue;

            await _questionAnswersRepository.InsertOneAsync(questionAnswer);
        }

        return Ok();
    }
}