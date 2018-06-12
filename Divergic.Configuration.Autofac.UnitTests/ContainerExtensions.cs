namespace Divergic.Configuration.Autofac.UnitTests
{
    using global::Autofac;

    public static class ContainerExtensions
    {
        public static ContainerAssertions Should(this IContainer instance)
        {
            return new ContainerAssertions(instance);
        }
    }
}