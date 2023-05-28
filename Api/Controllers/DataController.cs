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

    [HttpPost]
    public async Task<IActionResult> GetAnswer([FromBody]GetAnswerRequest request)
    {
        var code = FormatHelper.ConvertToCode(request.Question);

        var existingQuestion = await _questionsRepository.FindOneAsync(x => x.QuestionCode == code);

        if (existingQuestion == null)
        {
            var question = new Question()
            {
                Answers = request.Answers,
                QuestionText = request.Question,
                QuestionCode = FormatHelper.ConvertToCode(request.Question),
            };

            await _questionsRepository.InsertOneAsync(question);
        }

        var questionAnswer = await _questionAnswersRepository.FindOneAsync(x => x.QuestionCode == code);

        if (questionAnswer == null)
            return Json(new AnswerResponse()
            {
                Answer = -1
            });
        
        request.Answers = request.Answers.Select(FormatHelper.ConvertToCode).ToList();
        var result = request.Answers.IndexOf(questionAnswer.AnswerCode);

        if (result == -1)
        {
            await _questionAnswersRepository.HardDeleteManyAsync(x => x.Id == questionAnswer.Id);
            Console.WriteLine($"\n-----------------------Removed incorrect question answer {questionAnswer.QuestionCode}\n-----------------------");
        }
        
        return Json(new AnswerResponse()
        {
            Answer = result
        });
    }

    [HttpPost]
    public async Task<IActionResult> SubmitTest([FromBody]SubmitTestModel model)
    {
        if (model.Mark < 16)
            return Ok();
        
        var sure = model.Mark;

        var questions = model.Questions.Select(x => new Question()
        {
            QuestionText = x.Question,
            QuestionCode = FormatHelper.ConvertToCode(x.Question),
            Answers = x.Answers
        });

        var questionAnswers = model.Questions.Select(x => new QuestionAnswer()
        {
            Sure = sure,
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
            if(existingQuestionAnswer == null)
                await _questionAnswersRepository.InsertOneAsync(questionAnswer);
            else
            {
                if (existingQuestionAnswer.Sure < sure)
                {
                    existingQuestionAnswer.AnswerCode = questionAnswer.AnswerCode;
                    existingQuestionAnswer.Sure = sure;

                    await _questionAnswersRepository.ReplaceOneAsync(existingQuestionAnswer);
                }
            }

        }

        return Ok();
    }
}
