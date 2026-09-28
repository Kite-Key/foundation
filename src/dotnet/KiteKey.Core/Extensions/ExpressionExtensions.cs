using System.Linq.Expressions;

namespace KiteKey.Core;

/// <summary>Extensions for <see cref="Expression" /></summary>
public static class ExpressionExtensions
{
	/// <summary>Get the name of the property/field being accessed in the expression</summary>
	/// <param name="source">Expression that accesses a property or field</param>
	/// <returns>Member name</returns>
	/// <example>
	///     <c><![CDATA[source = () => myObject.SomeProperty]]></c>
	/// </example>
	public static string GetMemberName(this Expression source)
	{
		if(source is not LambdaExpression sourceLambda)
			throw new ArgumentException("Must be a lambda expression", nameof(source));

		Expression body = sourceLambda.Body;
		if(body is not MemberExpression bodyMember)
			throw new ArgumentException("Must be a member access expression", nameof(source));

		return bodyMember.Member.Name;
	}
}
