using System;
using System.IO;
using System.Linq;

namespace TSP_Algorithm
{
    class Program
    {
        private static Random rand = new Random();
        private static int rowCounter;
        private static int randomCounter;
        private static int iterationCounter;
        private static int crossingParameter;
        private static int mutationParameter;
        private static int bestResult = int.MaxValue;
        private static int[] bestRoute;

        //tournament selection https://en.wikipedia.org/wiki/Tournament_selection
        private static int[,] tournamentSelection(int[] sumRoadTab, int[,] randomDigitsTab)
        {
            int[,] tempTab = new int[randomDigitsTab.GetLength(0), randomDigitsTab.GetLength(1)];

            for (int i = 0; i < sumRoadTab.Length; i++)
            {
                int rand1 = rand.Next(0, sumRoadTab.Length);
                int rand2 = rand.Next(0, sumRoadTab.Length);

                int temp = 0;
                if (sumRoadTab[rand1] > sumRoadTab[rand2])
                {
                    temp = rand2;
                }
                else
                {
                    temp = rand1;
                }
                for (int j = 0; j < randomDigitsTab.GetLength(1); j++)
                {
                    tempTab[i, j] = randomDigitsTab[temp, j];
                }
            }
            return tempTab;
        }
        //PMX crossing method http://www.rubicite.com/Tutorials/GeneticAlgorithms/CrossoverOperators/PMXCrossoverOperator.aspx
        private static int[,] crossPMX(int[,] population, int parameter)
        {
            int[,] personsPopulation = new int[population.GetLength(0), population.GetLength(1)];

            for (int i = 0; i < population.GetLength(0); i += 2)
            {
                if (rand.Next(0, 101) <= parameter)
                {
                    int[] person1 = new int[population.GetLength(1)];
                    int[] person2 = new int[population.GetLength(1)];

                    for (int j = 0; j < population.GetLength(1); j++)
                    {
                        person1[j] = population[i, j];
                        person2[j] = population[i + 1, j];
                    }

                    int[,] persons = crossPair(person1, person2);

                    for (int j = 0; j < persons.GetLength(1); j++)
                    {
                        personsPopulation[i, j] = persons[0, j];
                        personsPopulation[i + 1, j] = persons[1, j];
                    }
                }
                else
                {
                    for (int j = 0; j < population.GetLength(1); j++)
                    {
                        personsPopulation[i, j] = population[i, j];
                        personsPopulation[i + 1, j] = population[i + 1, j];
                    }
                }
            }
            return personsPopulation;
        }
        private static int[,] crossPair(int[] person1, int[] person2)
        {
            int[] tempTab1 = new int[person1.Length];
            int[] tempTab2 = new int[person2.Length];

            int[,] persons = new int[2, person1.Length];
            //input to each persons -1, so it can be changed in next steps
            for (int i = 0; i < person1.Length; i++)
            {
                tempTab1[i] = -1;
                tempTab2[i] = -1;
            }
            //start and end of the section 
            int ppp = rand.Next(0, person1.Length - 1);
            int dpp = rand.Next(ppp + 1, person1.Length);

            for (int i = ppp; i <= dpp; i++)
            {
                tempTab1[i] = person1[i];
                tempTab2[i] = person2[i];
            }
            //change genes(evrything except from ppp to dpp)
            tempTab1 = changeGenes(person1, tempTab1, ppp, dpp, true);
            tempTab1 = changeGenes(person1, tempTab1, ppp, dpp, false);
            tempTab2 = changeGenes(person2, tempTab2, ppp, dpp, true);
            tempTab2 = changeGenes(person2, tempTab2, ppp, dpp, false);

            for (int i = 0; i < persons.GetLength(1); i++)
            {
                persons[0, i] = tempTab1[i];
                persons[1, i] = tempTab2[i];
            }
            return persons;
        }
        //changing genes method
        private static int[] changeGenes(int[] parent, int[] person, int ppp, int dpp, bool start)
        {
            //from 0 to ppp
            int from = 0;
            int to = ppp;
            //frokm dpp+1 to end
            if (!start)
            {
                from = dpp + 1;
                to = person.Length;
            }
            //changing genes between parent and descendant
            for (int i = from; i < to; i++)
            {
                int gene = parent[i];
                while (person.Contains(gene))
                {
                    gene = parent[Array.IndexOf(person, gene)];
                }
                person[i] = gene;
            }
            return person;
        }
        //mutation https://en.wikipedia.org/wiki/Mutation_(genetic_algorithm)
        private static int[,] Mutation(int[,] crossedTab)
        {
            int randomMutation = rand.Next(0, 101);
            int[,] mutatedTab = new int[randomCounter, rowCounter];

            for (int i = 0; i < randomCounter; i++)
            {
                for (int j = 0; j < rowCounter; j++)
                {
                    mutatedTab[i, j] = crossedTab[i, j];
                }

                if (randomMutation <= mutationParameter)
                {
                    int ppp = rand.Next(1, rowCounter - 1);
                    int dpp = rand.Next(1, rowCounter - 1);
                    int temp;

                    while (ppp == dpp)
                    {
                        dpp = rand.Next(1, rowCounter - 1);
                    }
                    if (ppp > dpp)
                    {
                        temp = ppp;
                        ppp = dpp;
                        dpp = temp;
                    }
                    int counter = ppp;

                    for (int j = dpp; j >= ppp; j--)
                    {
                        mutatedTab[i, counter] = crossedTab[i, j];
                        counter++;
                    }
                }
            }
            return mutatedTab;
        }
        static void Main(string[] args)
        {
            //setting up patch to our txt file that contains distance to cities
            string path = @"C:\Users\Admin\Desktop\path.txt";
            string text = File.ReadAllText(path);
            string[] rows = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            int counter = 0;
            int dig = 0;
            rowCounter = int.Parse(rows[0]);
            bestRoute = new int[rowCounter];
            int[,] basicTab = new int[rowCounter, rowCounter];
            //saving data from txt file to tab
            for (int i = 1; i < rows.Length - 1; i++)
            {
                string[] baseTab = rows[i].Split(' ');
                for (int j = 0; j < baseTab.Length - 1; j++)
                {
                    int.TryParse(baseTab[j], out dig);
                    basicTab[counter, j] = dig;
                }
                counter++;
            }
            Console.WriteLine();
            //symetrical inversion
            for (int i = rowCounter - 1; i > 0; i--)
            {
                for (int j = 0; j < rowCounter; j++)
                {
                    basicTab[j, i] = basicTab[i, j];
                }
            }
            Console.WriteLine("Enter quantity of subjects : ");
            randomCounter = int.Parse(Console.ReadLine());
            int[,] randomDigits = new int[randomCounter, rowCounter];

            //draw routes without repetition
            for (int i = 0; i < randomCounter; i++)
            {
                for (int j = 0; j < rowCounter; j++)
                {
                    randomDigits[i, j] = rand.Next(0, rowCounter);
                    for (int k = 0; k < j; k++)
                    {
                        if (j != k && randomDigits[i, j] == randomDigits[i, k])
                        {
                            j--;
                            break;
                        }
                    }
                }
            }

            Console.WriteLine("Enter quantity of iterations");
            iterationCounter = int.Parse(Console.ReadLine());
            Console.WriteLine("Enter crossing parameter 1-100");
            crossingParameter = int.Parse(Console.ReadLine());
            Console.WriteLine("Enter mutation parameter 1-100");
            mutationParameter = int.Parse(Console.ReadLine());
            for (int x = 0; x <= iterationCounter + 1; x++)
            {

                //sum of routes
                int rank = 0;
                int[] rankTab = new int[randomCounter];
                for (int i = 0; i < randomCounter; i++)
                {
                    rank = 0;
                    for (int j = 0; j < rowCounter; j++)
                    {
                        int next = j + 1;
                        if (next >= rowCounter)
                        {
                            next = 0;
                        }
                        int counter1 = randomDigits[i, j];
                        int counter2 = randomDigits[i, next];

                        rank += basicTab[counter1, counter2];

                    }
                    rankTab[i] = rank;
                }

                int[,] tempTab = tournamentSelection(rankTab, randomDigits);
                int[,] crossedTab = crossPMX(tempTab, crossingParameter);
                int[,] mutatedDigits = Mutation(crossedTab);
                //sum routes after mutation
                int summary = 0;
                int[] summaryTab = new int[randomCounter];
                for (int i = 0; i < randomCounter; i++)
                {
                    summary = 0;
                    for (int j = 0; j < rowCounter; j++)
                    {
                        int next = j + 1;
                        if (next == rowCounter)
                        {
                            next = 0;
                        }
                        int counter1 = mutatedDigits[i, j]; //start
                        int counter2 = mutatedDigits[i, next]; //end

                        summary += basicTab[counter1, counter2];

                    }
                    summaryTab[i] = summary;
                    if (bestResult > summaryTab[i])
                    {
                        bestResult = summaryTab[i];
                        for (int j = 0; j < rowCounter; j++)
                        {
                            bestRoute[j] = mutatedDigits[i, j];
                        }
                    }
                }
                //select best route (smallest)

                if (x == iterationCounter)
                {
                    Console.WriteLine("Best result: " + bestResult);
                    Console.WriteLine("Best route");
                    for (int i = 0; i < bestRoute.Length; i++)
                    {
                        Console.Write(bestRoute[i]);
                        if (i != bestRoute.Length - 1)
                        {
                            Console.Write("-");
                        }
                    }
                    Console.WriteLine();
                    Console.ReadKey();
                }
            }
        }
}
