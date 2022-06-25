
import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { TabDirective, TabsetComponent } from 'ngx-bootstrap/tabs';
import { Member } from 'src/app/_models/Member';
import { Message } from 'src/app/_models/Message';
import { MembersService } from 'src/app/_services/members.service';
import { MessageService } from 'src/app/_services/message.service';


@Component({
  selector: 'app-member-detail',
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css']
})
export class MemberDetailComponent implements OnInit {
  @ViewChild("memberTabs", { static: true }) memberTabs: TabsetComponent;
  member: Member;
  imageObject: Array<object>
  activeTab: TabDirective;
  messages: Message[] = []


  constructor(private memberService: MembersService, private route: ActivatedRoute, private messageService: MessageService) {

  }

  ngOnInit(): void {
    this.route.data.subscribe((response) => {
      this.member = response.member
    })
    this.route.queryParams.subscribe((response) => {
      response.tab ? this.selectTab(response.tab) : this.selectTab(0)
    })
    this.imageObject = this.getImages()



  }
  getImages(): Array<object>[] {
    const imageUrls = [];
    for (const photo of this.member.photos) {

      imageUrls.push({
        image: photo.url,
        thumbImage: photo.url
      })
    }
    return imageUrls;
  }
  addLike(username: string) {
    this.memberService.addLike(username).subscribe()
  }
  loadMessages() {
    this.messageService.getMessageThread(this.member.username).subscribe((response: Message[]) => {
      this.messages = response
    })
  }
  onTabActivated(data: TabDirective) {
    this.activeTab = data
    if (this.activeTab.heading === "Messages" && this.messages.length == 0) {
      this.loadMessages()
    }
  }
  selectTab(tabId: number) {
    this.memberTabs.tabs[tabId].active = true

  }

}
