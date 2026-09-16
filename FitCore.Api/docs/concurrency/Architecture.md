# Architecture 

For using locks and escaping the dead lock there should be a global rules on order - [Stores/Locking] Stores/Locking.
Every consumer that needs to read something and lock's the row before write has to obtain the transaction from this class and on every resource locking there is a priority of order that follows all concurrent consumers, obtaining one lock increases the scoped priority sum and the next resource must be with bigger priority if not then exception is thrown.  