using System.Reflection;

namespace SRSS.Project.Infrastructure
{
    public class AssemblyReference
    {
        public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
    }
}