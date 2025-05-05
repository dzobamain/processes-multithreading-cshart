using System;
using System.Threading;

class Program
{
    static AutoResetEvent perfectEvent = new AutoResetEvent(false);
    static AutoResetEvent fiboEvent = new AutoResetEvent(false);
    static AutoResetEvent perfectDoneEvent = new AutoResetEvent(false);
    static AutoResetEvent fiboDoneEvent = new AutoResetEvent(false);

    static int currentNumber;
    static int processedCount = 0;
    static int totalCount;
    static object lockObj = new object();

    static void Main()
    {
        Console.Write("How many numbers to generate? ");
        if (!int.TryParse(Console.ReadLine(), out totalCount) || totalCount <= 0)
        {
            Console.WriteLine("Invalid number.");
            return;
        }

        Thread tCheckPerfect = new Thread(CheckPerfect) { IsBackground = true };
        Thread tCheckFibonacci = new Thread(CheckFibonacci) { IsBackground = true };
        Thread tProgressBar = new Thread(ProgressBar) { IsBackground = true };
        Thread tGenerate = new Thread(Generate) { IsBackground = false };

        tCheckPerfect.Start();
        tCheckFibonacci.Start();
        tProgressBar.Start();
        tGenerate.Start(totalCount);

        tGenerate.Join(); 

        Console.WriteLine("Generation finished.");
    }

    static void Generate(object maxValue)
    {
        Random rand = new Random();
        for (int i = 1; i <= totalCount; i++)
        {
            int num = rand.Next(1, (int)maxValue);
            lock (lockObj) { currentNumber = num; }

            Console.WriteLine($"Generated (No{i}): {num}");

            perfectEvent.Set();
            fiboEvent.Set();
            perfectDoneEvent.WaitOne();
            fiboDoneEvent.WaitOne();

            lock (lockObj) { processedCount++; }
        }
    }

    static void CheckPerfect()
    {
        while (true)
        {
            perfectEvent.WaitOne();
            int num;
            lock (lockObj) { num = currentNumber; }

            if (IsPerfect(num))
                Console.WriteLine($"\t{num} is a perfect number");

            perfectDoneEvent.Set();
        }
    }

    static void CheckFibonacci()
    {
        while (true)
        {
            fiboEvent.WaitOne();
            int num;
            lock (lockObj) { num = currentNumber; }

            if (IsFibonacci(num))
                Console.WriteLine($"\t{num} is a Fibonacci number");

            fiboDoneEvent.Set();
        }
    }

    static void ProgressBar()
    {
        const int barWidth = 40;
        while (processedCount < totalCount)
        {
            int done;
            lock (lockObj) { done = processedCount; }
            double fraction = (double)done / totalCount;
            int filled = (int)(fraction * barWidth);

            Console.Write("\r[");
            Console.Write(new string('█', filled));
            Console.Write(new string('─', barWidth - filled));
            Console.Write($"] {done}/{totalCount}");
            
            Thread.Sleep(100);
        }

        Console.Write("\r[");
        Console.Write(new string('█', barWidth));
        Console.Write($"] {totalCount}/{totalCount}");
    }

    static bool IsPerfect(int n)
    {
        if (n < 2) 
            return false;

        int sum = 1;
        for (int i = 2; i <= n / 2; i++)
            if (n % i == 0) 
                sum += i;

        return sum == n;
    }

    static bool IsFibonacci(int n)
    {
        long a = 5L * n * n + 4;
        long b = 5L * n * n - 4;

        return IsPerfectSquare(a) || IsPerfectSquare(b);
    }

    static bool IsPerfectSquare(long x)
    {
        long r = (long)Math.Sqrt(x);

        return r * r == x;
    }
}
