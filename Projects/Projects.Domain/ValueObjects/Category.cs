using Projects.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Domain.ValueObjects
{
	public sealed class Category : ValueObject
	{
		public string Value { get; }

		private static readonly HashSet<string> Allowed = new()
		{
			"design",           // UI/UX, графика
			"development",      // программирование, инженерия
			"frontend",         // интерфейс
			"backend",          // серверная логика
			"fullstack",        // комплексная разработка
			"mobile",           // Android/iOS
			"devops",           // CI/CD, инфраструктура
			"qa",               // тестирование
			"testing",          // синоним QA
			"research",         // исследования
			"analytics",        // анализ данных
			"data-science",     // наука о данных
			"machine-learning", // ИИ и ML
			"ai",               // искусственный интеллект
			"security",         // безопасность
			"sysadmin",         // администрирование
			"game-dev",         // разработка игр
			"project-management", // управление проектами
			"product-management", // управление продуктом
			"marketing",        // маркетинг
			"smm",              // соцсети
			"seo",              // поисковая оптимизация
			"content",          // контент-менеджмент
			"copywriting",      // написание текстов
			"sales",            // продажи
			"support",          // поддержка клиентов
			"legal",            // юридическая поддержка
			"finance",          // финансы
			"hr",               // кадры
			"training",         // обучение и развитие
			"operations",       // операционная деятельность
			"strategy",         // стратегическое планирование
			"architecture",     // архитектура систем
			"consulting",       // консалтинг
			"innovation",       // инновации
			"compliance"        // соответствие нормам
		};

		private Category(string value) => Value = value;

		public static Category From(string input)
		{
			if (string.IsNullOrWhiteSpace(input))
				throw new ArgumentException("Category is required.");

			var val = input.Trim().ToLowerInvariant();
			if (!Allowed.Contains(val))
				throw new ArgumentException($"Invalid category: {val}");

			return new Category(val);
		}

		protected override IEnumerable<object> GetEqualityComponents()
		{
			yield return Value;
		}

		public override string ToString() => Value;
	}

}
