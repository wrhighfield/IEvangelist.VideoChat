import { Component, ViewChild, OnInit } from '@angular/core';
import { Room, LocalTrack, LocalVideoTrack, LocalAudioTrack, RemoteParticipant } from 'twilio-video';
import { RoomsComponent } from '../rooms/rooms.component';
import { CameraComponent } from '../camera/camera.component';
import { SettingsComponent } from '../settings/settings.component';
import { ParticipantsComponent } from '../participants/participants.component';
import { VideoChatService } from '../services/videochat.service';
import { Profile, AccountService } from '../services/account.service';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@aspnet/signalr';

@Component({
    selector: 'app-home',
    styleUrls: ['./home.component.css'],
    templateUrl: './home.component.html',
})
export class HomeComponent implements OnInit {
    @ViewChild('rooms') rooms: RoomsComponent;
    @ViewChild('camera') camera: CameraComponent;
    @ViewChild('settings') settings: SettingsComponent;
    @ViewChild('participants') participants: ParticipantsComponent;

    private notificationHub: HubConnection;
    private activeRoom: Room;

    user: Profile;

    get host() {
        return location.origin;
    }

    constructor(
        private readonly videoChatService: VideoChatService,
        private readonly accountService: AccountService) { }

    async ngOnInit() {
        this.user = await this.accountService.getProfile();

        const builder =
            new HubConnectionBuilder()
                .configureLogging(LogLevel.Trace)
                .withUrl(`${location.origin}/notificationHub`);

        this.notificationHub = builder.build();
        this.notificationHub.on('OnRoomsUpdated', async updated => {
            if (updated) {
                await this.rooms.updateRooms();
            }
        });
        this.notificationHub.start();
    }

    async onSettingsChanged(deviceInfo: MediaDeviceInfo) {
        await this.camera.initializePreview(deviceInfo);
    }

    async onRoomChanged(roomName: string) {
        if (roomName) {
            if (this.activeRoom) {
                this.activeRoom.disconnect();
            }

            this.activeRoom =
                await this.videoChatService
                          .joinOrCreateRoom(roomName, this.camera.tracks);

            this.participants.initialize(this.activeRoom.participants);

            this.activeRoom
                .on('disconnected',
                    (room: Room) => {
                        room.localParticipant.tracks.forEach(publication => {
                            if (this.isDetachable(publication.track)) {
                                const attachedElements = publication.track.detach();
                                attachedElements.forEach(element => element.remove());
                            }
                        });
                    })
                .on('participantConnected',
                    (participant: RemoteParticipant) => this.participants.add(participant))
                .on('participantDisconnected',
                    (participant: RemoteParticipant) => this.participants.remove(participant))
                .on('dominantSpeakerChanged',
                (dominantSpeaker: RemoteParticipant) => this.participants.loudest(dominantSpeaker));

            this.notificationHub.send('RoomsUpdated', true);
        }
    }

    async onSignOut() {
        await this.accountService.signOut();
    }

    private isDetachable(track: LocalTrack): track is LocalAudioTrack | LocalVideoTrack {
        return !!track
            && (track as LocalAudioTrack).detach !== undefined
            || (track as LocalVideoTrack).detach !== undefined;
    }
}