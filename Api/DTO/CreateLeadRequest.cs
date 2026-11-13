using System.ComponentModel.DataAnnotations;
using EducationCenter.Enums;

namespace EducationCenter.DTO;

public class CreateLeadRequest
{
	[Required(ErrorMessage = "Укажите имя")]
	public string Name { get; set; } = string.Empty;

	[Required(ErrorMessage = "Укажите телефон")]
	public string Phone { get; set; } = string.Empty;

	[Required(ErrorMessage = "Укажите Email")]
	[EmailAddress(ErrorMessage = "Некорректный Email.")]
	public string Email { get; set; } = string.Empty;

	[EnumDataType(typeof(ClientType), ErrorMessage = "Некорректный тип клиента")]
	public ClientType ClientType { get; set; }
}