# States on entities and their domain relations

## Types of states

States have two meanings: action and informative.
Action meaning is what the application cares about is this object state allowed to take a certain action is the question that is asked.
Informative state is description of the object lifetime from which statistics can be derived later, one state two meanings.
From application standpoint the state is binary either can make or can not make certain action.

## Business requirements

Down are listed valid contexts which are domain driven specific.

1. `Active member` -> active membership -> has sessions to be resolved or to be reserved.
2. `Paused member` -> cancelled/expired membership -> no reserved sessions.
3. `Lead member` -> no membership -> no reserved sessions or resolved.
4. `Cancelled member` -> cancelled/expired memberships -> no reserved sessions.

## Dependency direction

`Member` -> `Membership` -> `Visit` -> `Outside Actions`

In this path of depedency the visits are scheduled, voided, cancelled and different actions are applied that always are checking the `boundary rules` for changing membership state which on a condition is changed for example to expired and therefore the chain continous onto the `boundary rules` of member which need to be passed for change state to be applied.

Each action from right to left is working into self domain and checks the boundry on the left (`the dependend`).

### Boundries

1. For `visit the boundry` is action from outside and checking the state transition rules - example, coach tries to void a cancelled visit, the action ends on the domain of visit with error, if the coach completes a schedule visit - valid action, then the domain rule is passed and goes to the membership boundry.

2. `Membership boundry` is if the membership burned all visits changes the state to expired then knocks on the member boundry.

3. `Member boundry` lists all memberships statuses and if there is not single one active changes state to paused.

## Cross cutting actions 

Cross-cutting actions are dedicated cancellations of a member or a membership. To keep the domain context valid, resolution runs right to left.

Before a member can move Paused/Lead → Cancelled, every membership it depends on must already be in a resolved state: any Active membership becomes Cancelled; other resolved states stay as they are. That step is only safe after the objects those memberships depend on - visits - are resolved: existing resolved visits stay unchanged: any Scheduled visit becomes Cancelled.

Only then is the context protected and the member can be cancelled safely.   


## Design choice 

As we can see here we have derived states and source of truth state that is denormalization, that can make sync problem if is not designed right starting from the application level to the DB, i decided to objects to carry their derived bag with themselfs so later re-computation should not be required for BI to give the right insight data to the business.

## Concurenncy 

How this design is protected with concurrency is described in [../concurrency/invariants.md].




