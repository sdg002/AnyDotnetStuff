from apscheduler.schedulers.blocking import BlockingScheduler
import datetime

sched = BlockingScheduler()

@sched.scheduled_job(trigger='cron', id='my_job_id', second=30)
def some_decorated_task():
    print(f"Inside function some_decorated_task  current time is:{datetime.datetime.now()}")

print("Hello world - this is an example of scheduled jobs via AP Scheduler")
sched.start()
    
