using System;
using System.Collections.Generic;


namespace ExpertSystemCarDiagnostics
{
    public class Rule
    {
        public string Name { get; set; }
        public Func<CarFacts, bool> Condition { get; set; }
        public string Conclusion { get; set; }
    }

    public class CarFacts
    {
        public bool StarterSound { get; set; }
        public bool HeadlightsOn { get; set; }
        public bool EngineKnock { get; set; }
        public bool GasolineSmell { get; set; }
        public bool EngineStartedAndStalled { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Экспертная система диагностики неисправностей автомобиля");
            Console.WriteLine("=======================================================\n");

            CarFacts facts = GetFactsFromUser();
            List<Rule> knowledgeBase = CreateKnowledgeBase();
            string diagnosis = RunInferenceEngine(facts, knowledgeBase);

            Console.WriteLine("\n" + new string('=', 50));
            Console.WriteLine("РЕЗУЛЬТАТ ДИАГНОСТИКИ:");
            Console.WriteLine(new string('=', 50));
            Console.WriteLine(diagnosis);
            Console.WriteLine(new string('=', 50));
        }

        static CarFacts GetFactsFromUser()
        {
            var facts = new CarFacts();

            Console.WriteLine("Ответьте на вопросы о состоянии автомобила (да/нет):");

            facts.StarterSound = GetYesNoAnswer("Слышен ли звук стартера при повороте ключа?");
            facts.HeadlightsOn = GetYesNoAnswer("Горят ли фары?");
            facts.EngineKnock = GetYesNoAnswer("Слышен ли стук в двигателе?");
            facts.GasolineSmell = GetYesNoAnswer("Чувствуется ли запах бензина?");
            facts.EngineStartedAndStalled = GetYesNoAnswer("Двигатель заводился и сразу заглох?");

            return facts;
        }

        static bool GetYesNoAnswer(string question)
        {
            while (true)
            {
                Console.Write(question + " (да/нет): ");
                string answer = Console.ReadLine().ToLower().Trim();

                if (answer == "да" || answer == "д" || answer == "yes" || answer == "y")
                    return true;
                else if (answer == "нет" || answer == "н" || answer == "no" || answer == "n")
                    return false;
                else
                    Console.WriteLine("Пожалуйста, ответьте 'да' или 'нет'");
            }
        }

        static List<Rule> CreateKnowledgeBase()
        {
            return new List<Rule>
            {
                new Rule
                {
                    Name = "Правило 1",
                    Condition = facts => !facts.StarterSound && !facts.HeadlightsOn,
                    Conclusion = "Проблема с аккумулятором. Возможно, он разряжен или клеммы окислились."
                },
                new Rule
                {
                    Name = "Правило 2",
                    Condition = facts => !facts.StarterSound && facts.HeadlightsOn,
                    Conclusion = "Неисправен стартер или втягивающее реле."
                },
                new Rule
                {
                    Name = "Правило 3",
                    Condition = facts => facts.StarterSound && !facts.GasolineSmell,
                    Conclusion = "Проблема в системе подачи топлива. Проверьте бензонасос, топливный фильтр."
                },
                new Rule
                {
                    Name = "Правило 4",
                    Condition = facts => facts.StarterSound && facts.GasolineSmell && facts.EngineStartedAndStalled,
                    Conclusion = "Возможна проблема с системой зажигания (свечи, катушка) или засоренность дроссельной заслонки."
                },
                new Rule
                {
                    Name = "Правило 5",
                    Condition = facts => facts.StarterSound && facts.GasolineSmell && facts.EngineKnock,
                    Conclusion = "СЕРЬЕЗНАЯ НЕИСПРАВНОСТЬ! Возможно, проблема с механической частью двигателя (ГРМ, поршневая). Немедленно прекратите попытки завестись и обратитесь к специалисту."
                },
                new Rule
                {
                    Name = "Правило 6",
                    Condition = facts => facts.StarterSound && facts.GasolineSmell &&
                                       !facts.EngineStartedAndStalled && !facts.EngineKnock,
                    Conclusion = "Недостаточно данных для точной диагностики. Рекомендуется проверить свечи зажигания, катушку и топливный насос."
                }
            };
        }

        static string RunInferenceEngine(CarFacts facts, List<Rule> knowledgeBase)
        {
            Console.WriteLine("\n" + new string('-', 50));
            Console.WriteLine("ПРОЦЕСС ДИАГНОСТИКИ:");
            Console.WriteLine(new string('-', 50));

            foreach (var rule in knowledgeBase)
            {
                bool isApplicable = rule.Condition(facts);
                Console.WriteLine($"{rule.Name}: {(isApplicable ? "ПРИМЕНИМО" : "не применимо")}");

                if (isApplicable)
                {
                    Console.WriteLine($"Сработало: {rule.Name}");
                    return rule.Conclusion;
                }
            }

            return "Не удалось диагностировать проблему. Обратитесь к специалисту для детальной проверки.";
        }
    }
}
