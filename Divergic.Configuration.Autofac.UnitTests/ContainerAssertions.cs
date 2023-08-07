namespace Divergic.Configuration.Autofac.UnitTests
{
    using System.Collections.Generic;
    using System.Linq;
    using FluentAssertions;
    using FluentAssertions.Execution;
    using FluentAssertions.Primitives;
    using global::Autofac;
    using global::Autofac.Core;

    public class ContainerAssertions :
        ReferenceTypeAssertions<IContainer, ContainerAssertions>
    {
        public ContainerAssertions(IContainer container) : base(container)
        {
        }

        public AndConstraint<ContainerAssertions> HaveRegistered<T>(
            string because = "", params object[] becauseArgs)
        {
            Execute.Assertion
                .BecauseOf(because, becauseArgs)
                .Given(() => Subject.ComponentRegistry.Registrations)
                .ForCondition(IsRegistered<T>)
                .FailWith("Expected {0} to have been registered with the container.", typeof(T).FullName);

            return new AndConstraint<ContainerAssertions>(this);
        }

        public AndConstraint<ContainerAssertions> NotHaveRegistered<T>(
            string because = "", params object[] becauseArgs)
        {
            Execute.Assertion
                .BecauseOf(because, becauseArgs)
                .Given(() => Subject.ComponentRegistry.Registrations)
                .ForCondition(registrations => IsRegistered<T>(registrations) == false)
                .FailWith("Expected {0} to not have been registered with the container.", typeof(T).FullName);

            return new AndConstraint<ContainerAssertions>(this);
        }

        private static bool IsRegistered<T>(IEnumerable<IComponentRegistration> registrations)
        {
            return registrations.Any(registration =>
                registration.Services.OfType<TypedService>().Any(service => service.ServiceType == typeof(T)));
        }

        protected override string Identifier => "container";
    }
}