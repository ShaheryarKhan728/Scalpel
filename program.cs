class Program
{
    static void Main()
    {
        int num1 = 15;
        int num2 = 8;

        int sum = num1 + num2;
        int difference = num1 - num2;
        int product = num1 * num2;
        int quotient = num1 / num2;
        int remainder = num1 % num2;

        Console.WriteLine($"Number 1: {num1}");
        Console.WriteLine($"Number 2: {num2}");
        Console.WriteLine($"Sum: {sum}");
        Console.WriteLine($"Difference: {difference}");
        Console.WriteLine($"Product: {product}");
        Console.WriteLine($"Quotient: {quotient}");
        Console.WriteLine($"Remainder: {remainder}");
    }
}