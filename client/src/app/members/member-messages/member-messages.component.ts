import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Message } from 'src/app/_models/Message';
import { MessageService } from 'src/app/_services/message.service';

@Component({
  selector: 'app-member-messages',
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.css']
})
export class MemberMessagesComponent implements OnInit {
  @Input() messages: Message[]
  @Input() username: string
  @ViewChild('messageForm') messageForm: NgForm
  content: string


  constructor(private messageService: MessageService) { }

  ngOnInit(): void {
  }
  sendMessage() {
    this.messageService.sendMessage(this.username, this.content).subscribe(message => {
      this.messages.push(message)
      this.messageForm.reset()
    })
  }



}
