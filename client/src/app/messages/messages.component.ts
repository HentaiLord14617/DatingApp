import { Component, OnInit } from '@angular/core';
import { Message } from '../_models/Message';
import { PaginatedResult, Pagination } from '../_models/pagination';
import { MessageService } from '../_services/message.service';

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.css']
})
export class MessagesComponent implements OnInit {
  pageNumber = 1;
  pageSize = 5;
  container = "Unread"
  pagination: Pagination
  messages: Message[]
  loading = false;


  constructor(private messageService: MessageService) { }

  ngOnInit(): void {
    this.loadMessages()
  }
  loadMessages() {
    this.loading = true
    this.messageService.getMessages(this.pageNumber, this.pageSize, this.container).subscribe((response) => {

      this.pagination = response.pagination
      this.messages = response.result
      this.loading = false
    })
  }
  pageChanged(event: any) {
    this.pageNumber = event.page
    this.loadMessages()
  }
  deleteMessage(id: number) {
    this.messageService.deleteMessage(id).subscribe(() => {
      this.messages.splice(this.messages.findIndex(message => message.id === id), 1)
    })
  }

}
