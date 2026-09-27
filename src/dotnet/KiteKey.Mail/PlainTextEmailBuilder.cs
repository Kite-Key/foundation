using System.Text;


namespace KiteKey.Mail;

/// <summary>Helps build standard emails.</summary>
public class PlainTextEmailBuilder
{
	private readonly StringBuilder _builder;

	/// <summary>DI Constructor. Assumes a body of 2500 characters.</summary>
	public PlainTextEmailBuilder()
		: this(2500) { }

	/// <summary>Constructor with explicit size</summary>
	/// <param name="size">Expected maximum message size</param>
	public PlainTextEmailBuilder(int size)
		=> _builder = new StringBuilder(size);

	/// <summary>Constructor with initial value</summary>
	/// <param name="initialValue">Initial message value</param>
	public PlainTextEmailBuilder(string initialValue)
		=> _builder = new StringBuilder(initialValue);

	/// <summary>Appends a field to the email body.</summary>
	/// <param name="header">The field title/label</param>
	/// <param name="value">The value of the data to display.</param>
	public void AppendField(string header, string? value = null)
	{
		_builder.AppendLine(header);
		_builder.AppendLine("---------------------------------");

		if(value is not null)
			_builder.AppendLine(value);

		_builder.AppendLine();
	}

	/// <summary>Appends a greeting to the emailed individual.</summary>
	/// <param name="name">Optional, the recipients name, used in the greeting if provided.</param>
	public void AppendGreeting(string? name = null)
	{
		_builder.Append("Hello");

		if(!string.IsNullOrEmpty(name))
			_builder.Append($" {name}");

		AppendLine(",");

		AppendLine();
	}

	/// <summary>Adds text (as is) to the email body. Used for things like the header/footer of the email. Adds a new line at the end.</summary>
	/// <param name="text">The text to add.</param>
	/// <param name="paragraph">If true, adds an empty line at the end of the paragraph.</param>
	public void AppendLine(string? text = null, bool paragraph = false)
	{
		if(string.IsNullOrEmpty(text))
			_builder.AppendLine();
		else
			_builder.AppendLine(text);

		if(paragraph)
			_builder.AppendLine();
	}

	/// <summary>Adds a signature to the email.</summary>
	/// <param name="name">The name to use in the signature line.</param>
	public void AppendSignature(string? name = null)
	{
		AppendLine("Best regards,");

		if(name is not null)
			AppendLine(name);
	}

	/// <summary>Renders the email body based on the previously supplied arguments.</summary>
	/// <returns>A string text of the email body.</returns>
	public override string ToString()
		=> _builder.ToString();
}
