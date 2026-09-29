using System.Runtime.CompilerServices;
using Avalonia.Controls.Templates;
using Microsoft.Extensions.DependencyInjection;

namespace Chatly.Desktop.Views;

[SingletonService]
internal sealed class ViewLocator : IDataTemplate
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<Type, Type> _viewTypes;
    private readonly ConditionalWeakTable<object, Control> _views = new();

    public ViewLocator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _viewTypes = typeof(ViewLocator).Assembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false })
            .SelectMany(viewType => viewType.GetInterfaces()
                .Where(contract => contract.IsGenericType &&
                                   contract.GetGenericTypeDefinition() == typeof(IViewFor<>))
                .Select(contract => (ViewModelType: contract.GenericTypeArguments[0], ViewType: viewType)))
            .ToDictionary(pair => pair.ViewModelType, pair => pair.ViewType);
    }

    public bool Match(object? data) => data is not null && _viewTypes.ContainsKey(data.GetType());

    public Control? Build(object? param) =>
        param is null ? null : _views.GetValue(param, CreateView);

    private Control CreateView(object viewModel) =>
        (Control)ActivatorUtilities.CreateInstance(_serviceProvider, _viewTypes[viewModel.GetType()], viewModel);
}