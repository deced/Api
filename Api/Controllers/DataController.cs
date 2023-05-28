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
    public async Task<IActionResult> GetAnswer([FromBody] GetAnswerRequest request)
    {
        var code = FormatHelper.ConvertToCode(request.Question);

        var existingQuestions = await _questionsRepository.FilterByAsync(x => x.QuestionCode == code);

        if (existingQuestions.Count() == 0)
        {
            var question = new Question()
            {
                Answers = request.Answers,
                QuestionText = request.Question,
                QuestionCode = FormatHelper.ConvertToCode(request.Question),
            };

            await _questionsRepository.InsertOneAsync(question);
        }
        else
        {
            if (existingQuestions.Any(x => x.Answers.TrueForAll(x => request.Answers.Contains(x))))
            {
            }
            else
            {
                var question = new Question()
                {
                    Answers = request.Answers,
                    QuestionText = request.Question,
                    QuestionCode = FormatHelper.ConvertToCode(request.Question),
                };

                await _questionsRepository.InsertOneAsync(question);
            }
        }

        var questionAnswers = (await _questionAnswersRepository.FilterByAsync(x => x.QuestionCode == code)).ToList();

        request.Answers = request.Answers.Select(FormatHelper.ConvertToCode).ToList();

        foreach (var questionAnswer in questionAnswers)
        {
            var index = request.Answers.IndexOf(questionAnswer.AnswerCode);

            if (index != -1)
            {
                if (questionAnswer.Sure == 20)
                    return Json(new AnswerResponse()
                    {
                        Answer = index
                    });
            }
        }

        return Json(new AnswerResponse()
        {
            Answer = -1
        });
    }

    [HttpPost]
    public async Task<IActionResult> SubmitTest([FromBody] SubmitTestModel model)
    {
        if (model.Mark < 16)
            return Ok();

        var sure = model.Mark;

        var questionAnswers = model.Questions.Select(x => new QuestionAnswer()
        {
            Sure = sure,
            QuestionCode = FormatHelper.ConvertToCode(x.Question),
            AnswerCode = FormatHelper.ConvertToCode(x.SelectedAnswer)
        });


        foreach (var questionAnswer in questionAnswers)
        {
            var existingQuestionAnswer =
                await _questionAnswersRepository.FindOneAsync(x =>
                    x.QuestionCode == questionAnswer.QuestionCode &&
                    x.AnswerCode == questionAnswer.AnswerCode);

            if (existingQuestionAnswer == null)
                await _questionAnswersRepository.InsertOneAsync(questionAnswer);
            else
            {
                if (existingQuestionAnswer.Sure < sure)
                {
                    // existingQuestionAnswer.AnswerCode = questionAnswer.AnswerCode;
                    existingQuestionAnswer.Sure = sure;

                    await _questionAnswersRepository.ReplaceOneAsync(existingQuestionAnswer);
                }
            }
        }

        return Ok();
    }
}