﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XamlingCore.Portable.Contract.Repos;
using XamlingCore.Portable.Contract.Services;
using XamlingCore.Portable.Model.Other;
using XamlingCore.Portable.Model.Response;
using XamlingCore.Portable.Model.Security;

namespace XamlingCore.Portable.Data.Security
{
    public class SecurityService : ISecurityService
    {
        private readonly ISecurityRepo _repo;

        public SecurityService(ISecurityRepo repo)
        {
            _repo = repo;
        }

        public async Task<XResult<XSecurityContext>> CreateContext(XSecurityContext parent, string name, 
            int permissions, Guid? owner = null, List<Guid> targetIds = null)
        {
            var context = new XSecurityContext
            {
                Id = Guid.NewGuid(),
                Name = name,
                Permissions = permissions, 
                Members = new List<Guid>(), 
                Children = new List<Guid>()
            };

            if(owner != null)
            {
                context.Members.Add(owner.Value);
            }

            if (targetIds != null)
            {
                context.Targets = targetIds;
            }

            parent?.Children.Add(context.Id);

            var newContextResult = await SetContext(context);

            if (parent != null)
            {
                var parentUpdateResult = await SetContext(parent);
                if (!parentUpdateResult)
                {
                    return parentUpdateResult.Copy<XSecurityContext>();
                }
            }

            if (!newContextResult)
            {
                return newContextResult.Copy<XSecurityContext>();
            }

            return new XResult<XSecurityContext>(context);
        }

        public async Task<XResult<bool>> SetContext(XSecurityContext context)
        {
            return await _repo.SetContext(context);
        }

        public async Task<XResult<XSecurityContext>> GetAccess(Guid userId, Guid targetId, int securityTypes)
        {
            var context = await GetContextByTarget(targetId);

            if (!context || context.Object.Count == 0)
            {
                return context.Copy<XSecurityContext>();
            }

            foreach (var xSecurityContext in context.Object)
            {
                var validatedChainResult = await _validateContextChain(xSecurityContext, userId, securityTypes);

                if (validatedChainResult)
                {
                    return new XResult<XSecurityContext>(xSecurityContext, true, validatedChainResult.Message);
                }
            }

            return XResult<XSecurityContext>.GetNotAuthorised($"No access chains success for target: {targetId}, user:{userId}, permissions: {securityTypes}. {context.Object.Count} contexts searched");
        }

        public async Task<XResult<bool>> AddMember(XSecurityContext context, Guid currentUserId, Guid memberId)
        {
            //first check the current user has permissions to edit security
            var canEditSecurityResult =
                await _validateContextChain(context, currentUserId, (int) XPermission.EditPermissions);

            if (!canEditSecurityResult)
            {
                return canEditSecurityResult;
            }

            var liveContext = await GetContextById(context.Id);

            if (!liveContext)
            {
                return liveContext.Copy<bool>();
            }

            if (!liveContext.Object.Members.Contains(memberId))
            {
                liveContext.Object.Members.Add(memberId);

                var setResult = await SetContext(liveContext.Object);

                if (!setResult)
                {
                    return setResult;
                }
            }

            context.Members.Add(currentUserId);

            return new XResult<bool>(true);
        }

        public async Task<XResult<bool>> RemoveMember(XSecurityContext context, Guid currentUserId, Guid memberId)
        {
            //first check the current user has permissions to edit security
            var canEditSecurityResult =
                await _validateContextChain(context, currentUserId, (int)XPermission.EditPermissions);

            if (!canEditSecurityResult)
            {
                return canEditSecurityResult;
            }

            var liveContext = await GetContextById(context.Id);

            if (!liveContext)
            {
                return liveContext.Copy<bool>();
            }

            if (liveContext.Object.Members.Contains(memberId))
            {
                liveContext.Object.Members.Remove(memberId);

                var setResult = await SetContext(liveContext.Object);

                if (!setResult)
                {
                    return setResult;
                }
            }

            context.Members.Add(currentUserId);

            return new XResult<bool>(true);
        }

        public async Task<XResult<XSecurityContext>> GetContextByName(string contextName)
        {
            var context = await _repo.GetContextByName(contextName);
            return context;
        }

        public async Task<XResult<XSecurityContext>> GetContextById(Guid contextId)
        {
            var context = await _repo.GetContextById(contextId);
            return context;
        }

        public async Task<XResult<List<XSecurityContext>>> GetContextByTarget(Guid contextId)
        {
            var context = await _repo.GetContextsByTargetId(contextId);
            return context;
        }

        public async Task<XResult<List<XSecurityContext>>> GetParentContexts(Guid contextId)
        {
            var context = await _repo.GetParentContexts(contextId);
            return context;
        }

        public async Task<XResult<bool>> _validateContextChain(XSecurityContext context, Guid userId, int securityTypes)
        {
            if (context.Members.Contains(userId) && (context.Permissions & securityTypes) != 0)
            {
                return new XResult<bool>(true, true, $"Authorised by {context.Name} ({context.Id})");
            }

            var parents = await GetParentContexts(context.Id);

            if (!parents || parents.Object == null || parents.Object.Count == 0)
            {
                return
                    XResult<bool>.GetNotAuthorised(
                        $"Could not find parent context on context {context.Name} ({context.Id}). looking for permisssions {securityTypes} ");
            }

            foreach (var parent in parents.Object)
            {
                var c = await _validateContextChain(parent, userId, securityTypes);

                if (c)
                {
                    return c;
                }
            }

            return
                    XResult<bool>.GetNotAuthorised(
                        $"Could not find parent context on context {context.Name} ({context.Id}). looking for permisssions {securityTypes} (last try)");
        }
    }
}