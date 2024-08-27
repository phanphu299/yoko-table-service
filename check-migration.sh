while true
do
if [ $( docker ps -a -f status=exited | grep :migration- | wc -l ) -eq $( docker ps -a | grep :migration- | wc -l ) ]; then
  break;
else
    echo 'waiting migration to be finished'
    echo 'total:' $(docker ps -a | grep :migration- | wc -l)
    echo 'completed:' $(docker ps -a -f status=exited | grep :migration- | wc -l)
    sleep 1;
fi
done
echo 'done';