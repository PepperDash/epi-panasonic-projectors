using System.Collections.Generic;
using Newtonsoft.Json;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Displays
{
	/// <summary>
	/// Plugin device configuration object
	/// </summary>
	/// <remarks>
	/// Rename the class to match the device plugin being created
	/// </remarks>
	[ConfigSnippet("\"properties\":{\"control\":{}")]
	public class PanasonicProjectorControllerConfig
	{
		/// <summary>
		/// Serializes the poll time value
		/// </summary>
		/// <remarks>
		/// This is an exmaple device plugin property.  This should be modified or deleted as needed for the plugin being built.
		/// </remarks>
		/// <value>
		/// PollTimeMs property gets/sets the value as a long
		/// </value>
		/// <example>
		/// <code>
		/// "properties": {
		///		"polltimeMs": 30000
		/// }
		/// </code>
		/// </example>
		[JsonProperty("pollTimeMs")]
		public long PollTimeMs { get; set; }

		/// <summary>
		/// Serializes the warning timeout value
		/// </summary>
		/// <remarks>
		/// This is an exmaple device plugin property.  This should be modified or deleted as needed for the plugin being built.
		/// </remarks>
		/// <value>
		/// WarningTimeoutMs property gets/sets the value as a long
		/// </value>
		/// <example>
		/// <code>
		/// "properties": {
		///		"warningTimeoutMs": 180000
		/// }
		/// </code>
		/// </example>
		[JsonProperty("warningTimeoutMs")]
		public long WarningTimeoutMs { get; set; }

		/// <summary>
		/// Serializes the error timeout value
		/// </summary>
		/// /// <remarks>
		/// This is an exmaple device plugin property.  This should be modified or deleted as needed for the plugin being built.
		/// </remarks>
		/// <value>
		/// ErrorTimeoutMs property gets/sets the value as a long
		/// </value>
		/// <example>
		/// <code>
		/// "properties": {
		///		"errorTimeoutMs": 300000
		/// }
		/// </code>
		/// </example>
		[JsonProperty("errorTimeoutMs")]
		public long ErrorTimeoutMs { get; set; }
	}
}