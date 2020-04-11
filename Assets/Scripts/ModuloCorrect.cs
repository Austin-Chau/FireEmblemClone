namespace CustomMath
{
    public class CustomMath
    {
        //Returns k mod n, always positive
        public static int positiveMod(int k, int n) { return ((k %= n) < 0) ? k + n : k; }
    }
}
