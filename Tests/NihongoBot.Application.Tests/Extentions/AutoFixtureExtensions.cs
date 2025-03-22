using AutoFixture.Dsl;

using System.Linq.Expressions;
using System.Reflection;

namespace NihongoBot.Application.Tests.Extentions;

public static class AutoFixtureExtensions
{
	public static IPostprocessComposer<T> WithPrivate<T, TProperty>(
		this IPostprocessComposer<T> composer,
		Expression<Func<T, TProperty>> propertyPicker,
		TProperty value)
	{
		ArgumentNullException.ThrowIfNull(composer);
		ArgumentNullException.ThrowIfNull(propertyPicker);

		if (propertyPicker.Body is not MemberExpression memberExpression ||
			memberExpression.Member.MemberType != MemberTypes.Property)
		{
			throw new ArgumentException("Expression must be a property expression", nameof(propertyPicker));
		}

		string propertyName = memberExpression.Member.Name;

		PropertyInfo propertyInfo = typeof(T).GetProperty(propertyName,
			BindingFlags.Instance | BindingFlags.NonPublic |
			BindingFlags.Public);

		if (propertyInfo == null)
		{
			throw new ArgumentException($"Property '{propertyName}' not found on type '{typeof(T).FullName}'",
				nameof(propertyPicker));
		}

		return composer.Do(builder => propertyInfo.SetValue(builder, value, null));
	}

	public static IPostprocessComposer<T> WithoutPrivate<T, TProperty>(
		this IPostprocessComposer<T> composer,
		Expression<Func<T, TProperty>> propertyPicker)
	{
		ArgumentNullException.ThrowIfNull(composer);
		ArgumentNullException.ThrowIfNull(propertyPicker);

		if (propertyPicker.Body is not MemberExpression memberExpression ||
			memberExpression.Member.MemberType != MemberTypes.Property)
		{
			throw new ArgumentException("Expression must be a property expression", nameof(propertyPicker));
		}

		string propertyName = memberExpression.Member.Name;

		PropertyInfo propertyInfo = typeof(T).GetProperty(propertyName,
			BindingFlags.Instance | BindingFlags.NonPublic |
			BindingFlags.Public);

		if (propertyInfo == null)
		{
			throw new ArgumentException($"Property '{propertyName}' not found on type '{typeof(T).FullName}'",
				nameof(propertyPicker));
		}

		if (propertyInfo.PropertyType.IsValueType)
		{
			// For value types, set the property to its default value
			return composer.Do(builder => propertyInfo.SetValue(builder, Activator.CreateInstance(propertyInfo.PropertyType), null));
		}

		// For reference types, set the property to null
		return composer.Do(builder => propertyInfo.SetValue(builder, null, null));
	}

	public static IPostprocessComposer<T> WithReadonlyCollection<T, TProperty>(
		this IPostprocessComposer<T> composer,
		Expression<Func<T, IEnumerable<TProperty>>> propertyPicker,
		List<TProperty> value)
	{
		ArgumentNullException.ThrowIfNull(composer);
		ArgumentNullException.ThrowIfNull(propertyPicker);

		if (propertyPicker.Body is not MemberExpression memberExpression ||
			memberExpression.Member.MemberType != MemberTypes.Property)
		{
			throw new ArgumentException("Expression must be a property expression", nameof(propertyPicker));
		}

		// Check if there is a backingField for the property otherwise set the property directly
		string propertyName = memberExpression.Member.Name;
		string backingFieldName = "_" + char.ToLower(propertyName[0]) + propertyName.Substring(1);

		FieldInfo fieldInfo = typeof(T).GetField(backingFieldName,
			BindingFlags.Instance | BindingFlags.NonPublic);

		if (fieldInfo == null)
		{
			// Check if there is a backingField for the property with a different name
			backingFieldName = "<" + propertyName + ">k__BackingField";
			fieldInfo = typeof(T).GetField(backingFieldName,
				BindingFlags.Instance | BindingFlags.NonPublic);
		}

		// If the fieldInfo is still null, throw an exception
		if (fieldInfo == null)
		{
			throw new ArgumentException($"Backing field '{backingFieldName}' not found for property '{propertyName}' on type '{typeof(T).FullName}'",
				nameof(propertyPicker));
		}

		return composer.Do(builder => fieldInfo.SetValue(builder, value));
	}
}
