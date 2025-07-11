using FluentResults;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Users.Application.Contracts;
using Users.Application.Users.Commands.UpdateUser;
using Users.Domain.Interfaces.Repositories;
using Users.Domain.ValueObjects;

namespace Users.Application.Users.Commands.UpdateUser
{
	public class UpdateUserCommandHandler
		: IRequestHandler<UpdateUserRequest, Result>
	{
		private readonly IUserRepository _userRepo;

		public UpdateUserCommandHandler(IUserRepository userRepo) => _userRepo = userRepo;

		public async Task<Result> Handle(
			UpdateUserRequest request,
			CancellationToken ct)
		{
			var user = await _userRepo.GetById(request.UserId, ct);
			if (user is null)
				return Result.Fail("User not found");
			var emailObj = new Email(request.Command.NewEmail);
			try
			{
				user.UpdateUser(
					request.Command.NewName,
					request.Command.NewGender,
					request.Command.NewBirthday,
					emailObj,  
					request.Command.ModifiedBy);
			}
			catch (Exception ex)
			{
				return Result.Fail(ex.Message);
			}

			await _userRepo.Update(user, ct);
			return Result.Ok();
		}
	}
}
