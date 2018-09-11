# Final-Year-Project

## Notes

1. Debugging OptiTrack with mouse control.
1.1. Deactivate Optitrack Rigid Body script and Optitrack Streaming Client script.
1.2. Uncomment player.transform.position = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 20.0f));

2. EmotivPro and CortexService cannot be open. Otherwise, multiple sessions on same headset error will occur.