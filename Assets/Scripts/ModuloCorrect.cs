namespace ModuloCorrect
{
    public class CustomMath
    {
        public static int mod(int k, int n) { return ((k %= n) < 0) ? k + n : k; }
    }
}
