﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Input
{
    /// <summary>
    /// Generic Input Action Rule for raising actions based on specific criteria.
    /// </summary>
    [Serializable]
    public struct InputActionRuleDualAxis : IInputActionRule<Vector2>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="baseAction">The Base Action that the rule will listen to.</param>
        /// <param name="ruleAction">The Action to raise if the criteria is met.</param>
        /// <param name="criteria">The criteria to check against for determining if the action should be raised.</param>
        public InputActionRuleDualAxis(MixedRealityInputAction baseAction, MixedRealityInputAction ruleAction, Vector2 criteria)
        {
            this.baseAction = baseAction;
            this.ruleAction = ruleAction;
            this.criteria = criteria;
        }

        [SerializeField]
        [Tooltip("The Base Action that the rule will listen to.")]
        private MixedRealityInputAction baseAction;

        /// <inheritdoc />
        public MixedRealityInputAction BaseAction => baseAction;

        [SerializeField]
        [Tooltip("The Action to raise if the criteria is met.")]
        private MixedRealityInputAction ruleAction;

        /// <inheritdoc />
        public MixedRealityInputAction RuleAction => ruleAction;

        [SerializeField]
        [Tooltip("The criteria to check against for determining if the action should be raised.")]
        private Vector2 criteria;

        /// <inheritdoc />
        public Vector2 Criteria => criteria;
    }
}