﻿using SmartCondoApi.Services.Email;
using SmartCondoApi.Services.LinkGenerator;
using SmartCondoApi.Services.User;

namespace SmartCondoApi.Controllers
{
    public class UserProfileControllerDependencies(IUserProfileService userProfileService, ILinkGeneratorService linkGeneratorService, IEmailService emailService, IEmailConfirmationService emailConfirmationService) : IUserProfileControllerDependencies
    {
        public IUserProfileService UserProfileService { get; } = userProfileService;
        public ILinkGeneratorService LinkGeneratorService { get; } = linkGeneratorService;
        public IEmailService EmailService { get; } = emailService;
        public IEmailConfirmationService EmailConfirmationService { get; } = emailConfirmationService;
    }
}
