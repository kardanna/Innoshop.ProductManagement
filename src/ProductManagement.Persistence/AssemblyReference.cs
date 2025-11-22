using System.Reflection;

namespace ProductManagement.Persistence;

public static class AssemblyReference
{
    public static Assembly Assembly => typeof(AssemblyReference).Assembly;
}