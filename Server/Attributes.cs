#region References
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#endregion

namespace Server
{
	[AttributeUsage(AttributeTargets.Property)]
	public class HueAttribute : Attribute
	{ }

	[AttributeUsage(AttributeTargets.Property)]
	public class BodyAttribute : Attribute
	{ }

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
	public class PropertyObjectAttribute : Attribute
	{ }

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class NoSortAttribute : Attribute
	{ }

	[AttributeUsage(AttributeTargets.Method)]
	public class CallPriorityAttribute : Attribute
	{
		public int Priority { get; }

		public CallPriorityAttribute(int priority)
		{
			Priority = priority;
		}
	}

	public class CallPriorityComparer : IComparer<MethodInfo>
	{
		public int Compare(MethodInfo x, MethodInfo y)
		{
			if (x == null && y == null)
			{
				return 0;
			}

			if (x == null)
			{
				return 1;
			}

			if (y == null)
			{
				return -1;
			}

			int xPriority = GetPriority(x);
			int yPriority = GetPriority(y);

			if (xPriority > yPriority)
				return 1;

			if (xPriority < yPriority)
				return -1;

			return 0;
		}

		private static int GetPriority(MethodInfo mi)
		{
			object[] objs = mi.GetCustomAttributes(typeof(CallPriorityAttribute), true);

            if (objs.Length == 0)
			{
				return 0;
			}

			CallPriorityAttribute attr = objs[0] as CallPriorityAttribute;

			if (attr == null)
			{
				return 0;
			}

			return attr.Priority;
		}
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class TypeAliasAttribute : Attribute
	{
		private readonly string[] m_Aliases;

		public string[] Aliases => m_Aliases;

		public TypeAliasAttribute(params string[] aliases)
		{
			m_Aliases = aliases;
		}
	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class ParsableAttribute : Attribute
	{ }

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum)]
	public class CustomEnumAttribute : Attribute
	{
		private readonly string[] m_Names;

		public string[] Names => m_Names;

		public CustomEnumAttribute(string[] names)
		{
			m_Names = names;
		}
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class DeleteConfirmAttribute : Attribute
	{
		public string Message { get; set; }

		public DeleteConfirmAttribute()
			: this("Are you sure you wish to delete this item?")
		{ }

		public DeleteConfirmAttribute(string message)
		{
			Message = message;
		}

		public static bool Find<T>(out string message) where T : class
		{
			return Find(typeof(T), out message);
		}

		public static bool Find(object o, out string message)
		{
			return Find(o.GetType(), out message);
		}

		public static bool Find(Type t, out string message)
		{
			object[] attrs = t.GetCustomAttributes(typeof(DeleteConfirmAttribute), true);

			if (attrs.Length > 0)
			{
				message = string.Join("\n", attrs.OfType<DeleteConfirmAttribute>().Select(a => a.Message));
				return true;
			}

			message = string.Empty;
			return false;
		}
	}

	[AttributeUsage(AttributeTargets.Constructor)]
	public class ConstructableAttribute : Attribute
	{
		public AccessLevel AccessLevel { get; set; }

		public ConstructableAttribute()
			: this(AccessLevel.Player) //Lowest accesslevel for current functionality (Level determined by access to [add)
		{ }

		public ConstructableAttribute(AccessLevel accessLevel)
		{
			AccessLevel = accessLevel;
		}
	}

	[AttributeUsage(AttributeTargets.Property)]
	public class CommandPropertyAttribute : Attribute
	{
		private readonly AccessLevel m_ReadLevel;
		private readonly AccessLevel m_WriteLevel;
		private readonly bool m_ReadOnly;

		public AccessLevel ReadLevel => m_ReadLevel;

		public AccessLevel WriteLevel => m_WriteLevel;

		public bool ReadOnly => m_ReadOnly;

		public CommandPropertyAttribute(AccessLevel level, bool readOnly)
		{
			m_ReadLevel = level;
			m_ReadOnly = readOnly;
		}

		public CommandPropertyAttribute(AccessLevel level)
			: this(level, level)
		{ }

		public CommandPropertyAttribute(AccessLevel readLevel, AccessLevel writeLevel)
		{
			m_ReadLevel = readLevel;
			m_WriteLevel = writeLevel;
		}
	}
}
