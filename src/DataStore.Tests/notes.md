WARNING: if you create the Datastore database through the emulator rather than in code 
OR
if you start the emulator with out the following flags you might have problems
/PartitionCount=200 (otherwise you will only be able to run 25 tests per run)
/EnablePreview (at time of writing hierarchical partition keys won't work)
/Port=XXXX (recommand not running on 8081 as it often conflicts with other stuff)

Honestly the emulator is a piece of crap, it's so slow, and it fails
all the time under load, and you have to rerun test that are fine, and 
if you reset the data, restart it from commandline again after it restarts itself
to get the right flags above set.


/*TODO 
ReadByIds
named groups regex
^(?'type'_tp_[A-Za-z0-9\.]+)(?'tenant'_tn_[A-Za-z0-9-]+)?(?'timeperiod'_tm_[A-Za-z0-9:]+)?(?'id'_id_[A-Za-z0-9-]+)?(_na)?$                     
*/