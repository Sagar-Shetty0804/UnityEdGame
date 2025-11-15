using System;
using System.Collections.Generic;

[Serializable]
public class QuestionData
{
    public int id;
    public string subject;
    public int difficulty;
    public string question;
    public string optionA;
    public string optionB;
    public string optionC;
    public string optionD;
    public string correctAnswer;
    public string explanation;
    
    public string GetOption(string optionLetter)
    {
        return optionLetter.ToUpper() switch
        {
            "A" => optionA,
            "B" => optionB,
            "C" => optionC,
            "D" => optionD,
            _ => ""
        };
    }
    
    public bool IsCorrectAnswer(string selectedOption)
    {
        return correctAnswer.ToUpper() == selectedOption.ToUpper();
    }
}

[Serializable]
public class QuestionDatabase
{
    public List<QuestionData> questions;
}